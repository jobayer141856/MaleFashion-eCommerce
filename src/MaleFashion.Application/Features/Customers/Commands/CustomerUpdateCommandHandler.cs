using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Customers.Commands
{
    public class CustomerUpdateCommandHandler : ICommandHandler<CustomerUpdateCommand, Customer>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CustomerUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Customer> Handle(CustomerUpdateCommand command, CancellationToken cancellationToken)
        {
            var isDuplicate = await _unitOfWork.CustomerRepository.IsDuplicateEmail(command.Email, command.Id, cancellationToken);
            if (isDuplicate)
            {
                throw new DuplicateDataException($"A customer with email '{command.Email}' already exists.");
            }

            // Load customer with Addresses included (tracked by EF Core)
            var customer = await _unitOfWork.CustomerRepository.GetCustomerDetailsById(command.Id, cancellationToken);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID '{command.Id}' not found.");
            }

            customer.FirstName = command.FirstName.Trim();
            customer.LastName = command.LastName.Trim();
            customer.Email = command.Email.Trim().ToLowerInvariant();
            customer.PhoneNumber = command.PhoneNumber.Trim();
            customer.DateOfBirth = command.DateOfBirth;
            customer.Gender = command.Gender;
            customer.UpdatedAt = DateTime.UtcNow;

            // --- Address Synchronization ---
            if (command.Addresses != null)
            {
                // IDs of valid existing addresses from the submitted form
                var submittedIds = command.Addresses
                    .Where(a => a.Id.HasValue && a.Id.Value != Guid.Empty)
                    .Select(a => a.Id!.Value)
                    .ToList();

                // 1. Remove addresses removed by admin in the UI
                var addressesToRemove = customer.Addresses
                    .Where(a => !submittedIds.Contains(a.Id))
                    .ToList();

                foreach (var addr in addressesToRemove)
                {
                    customer.Addresses.Remove(addr);
                }

                // 2. Update existing or add new addresses
                foreach (var dto in command.Addresses)
                {
                    if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
                    {
                        var existing = customer.Addresses.FirstOrDefault(a => a.Id == dto.Id.Value);
                        if (existing != null)
                        {
                            existing.Type = dto.Type;
                            existing.IsDefault = dto.IsDefault;
                            existing.UpdatedAt = DateTime.UtcNow;

                            // Update owned Address in-place (prevents EF Core table-splitting delete collision)
                            if (existing.Address == null)
                            {
                                existing.Address = new Address(
                                    dto.StreetAddress,
                                    dto.ApartmentSuite,
                                    dto.City,
                                    dto.State,
                                    dto.PostalCode,
                                    string.IsNullOrWhiteSpace(dto.Country) ? "Bangladesh" : dto.Country
                                );
                            }
                            else
                            {
                                existing.Address.StreetAddress = dto.StreetAddress;
                                existing.Address.ApartmentSuite = dto.ApartmentSuite;
                                existing.Address.City = dto.City;
                                existing.Address.State = dto.State;
                                existing.Address.PostalCode = dto.PostalCode;
                                existing.Address.Country = string.IsNullOrWhiteSpace(dto.Country) ? "Bangladesh" : dto.Country;
                            }
                        }
                    }
                    else
                    {
                        // Add new address: Id is omitted so EF Core marks it as Added
                        customer.Addresses.Add(new CustomerAddress
                        {
                            CustomerId = customer.Id,
                            Type = dto.Type,
                            IsDefault = dto.IsDefault,
                            Address = new Address(
                                dto.StreetAddress,
                                dto.ApartmentSuite,
                                dto.City,
                                dto.State,
                                dto.PostalCode,
                                string.IsNullOrWhiteSpace(dto.Country) ? "Bangladesh" : dto.Country
                            ),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // SaveAsync cleanly applies all tracked updates, inserts, and deletes.
            await _unitOfWork.SaveAsync(cancellationToken);

            return customer;
        }
    }
}