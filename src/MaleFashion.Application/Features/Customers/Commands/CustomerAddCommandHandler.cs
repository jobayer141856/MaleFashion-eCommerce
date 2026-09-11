using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Customers.Commands
{
    public class CustomerAddCommandHandler : ICommandHandler<CustomerAddCommand, Customer>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CustomerAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Customer> Handle(CustomerAddCommand command, CancellationToken cancellationToken)
        {
            // Only validate unique email if it is a registered user (Self or Admin)
            if (command.Origin != CustomerOrigin.Guest)
            {
                var isDuplicate = await _unitOfWork.CustomerRepository.IsDuplicateEmail(command.Email, null, cancellationToken);
                if (isDuplicate)
                {
                    throw new DuplicateDataException($"A customer with email '{command.Email}' already exists.");
                }
            }

            var customerId = IdentityGenerator.NewSequentialGuid();
            var customer = new Customer
            {
                Id = customerId,
                UserId = command.UserId,
                FirstName = command.FirstName.Trim(),
                LastName = command.LastName.Trim(),
                Email = command.Email.Trim().ToLowerInvariant(),
                PhoneNumber = command.PhoneNumber.Trim(),
                DateOfBirth = command.DateOfBirth,
                Gender = command.Gender,
                Origin = command.Origin,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(command.StreetAddress) && !string.IsNullOrWhiteSpace(command.City))
            {
                customer.Addresses.Add(new CustomerAddress
                {
                    Id = IdentityGenerator.NewSequentialGuid(),
                    CustomerId = customerId,
                    Type = AddressType.Shipping,
                    Address = new Address(
                        command.StreetAddress,
                        command.ApartmentSuite,
                        command.City,
                        command.State ?? string.Empty,
                        command.PostalCode ?? string.Empty,
                        command.Country ?? "Bangladesh"
                    ),
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.CustomerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return customer;
        }
    }
}