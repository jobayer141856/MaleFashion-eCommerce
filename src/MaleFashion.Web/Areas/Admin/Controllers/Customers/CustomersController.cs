using Cortex.Mediator;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Customers.Commands;
using MaleFashion.Application.Features.Customers.Queries;
using MaleFashion.Domain.Entities;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Customers;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CustomersController : Controller
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(
            ILogger<CustomersController> logger,
            IMediator mediator,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _userManager = userManager;
        }

        public IActionResult Index() => View();

        public IActionResult Create() => View(new CustomerModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Verify that login user doesn't already exist
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = $"A user account with email '{model.Email}' already exists.",
                            Type = ResponseTypes.Danger
                        });
                        return View(model);
                    }

                    // 2. Create ApplicationUser in AspNetUsers first
                    var user = new ApplicationUser
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        UserName = model.Email.Trim().ToLowerInvariant(),
                        Email = model.Email.Trim().ToLowerInvariant(),
                        PhoneNumber = model.PhoneNumber.Trim(),
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName.Trim(),
                        EmailConfirmed = true
                    };

                    var password = !string.IsNullOrWhiteSpace(model.Password) ? model.Password : "Customer@123";
                    var userResult = await _userManager.CreateAsync(user, password);

                    if (!userResult.Succeeded)
                    {
                        var errors = string.Join("; ", userResult.Errors.Select(e => e.Description));
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = $"Failed to create login account: {errors}",
                            Type = ResponseTypes.Danger
                        });
                        return View(model);
                    }

                    // 3. Assign Customer Role
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // 4. Create Customer in Customers table with Origin = CustomerOrigin.Admin
                    var command = _mapper.Map<CustomerAddCommand>(model);
                    command.UserId = user.Id;
                    command.Origin = CustomerOrigin.Admin;

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Customer and login credentials successfully created.",
                        Type = ResponseTypes.Success
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException dex)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = dex.Message,
                        Type = ResponseTypes.Danger
                    });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to create customer.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = errorMessage,
                        Type = ResponseTypes.Danger
                    });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                // Use GetCustomerDetailsByIdQuery so Addresses are eagerly loaded!
                var query = new GetCustomerDetailsByIdQuery { Id = id };
                var customer = await _mediator.SendQueryAsync(query);

                if (customer == null) throw new Exception("Customer not found.");

                var model = new CustomerModel
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    Gender = customer.Gender,
                    DateOfBirth = customer.DateOfBirth,
                    Addresses = customer.Addresses.Select(a => new CustomerAddressModel
                    {
                        Id = a.Id,
                        Type = a.Type,
                        StreetAddress = a.Address.StreetAddress,
                        ApartmentSuite = a.Address.ApartmentSuite,
                        City = a.Address.City,
                        State = a.Address.State,
                        PostalCode = a.Address.PostalCode,
                        Country = a.Address.Country,
                        IsDefault = a.IsDefault
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load customer.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CustomerModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = new CustomerUpdateCommand
                    {
                        Id = model.Id,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Addresses = model.Addresses.Select(a => new CustomerAddressDto
                        {
                            Id = a.Id,
                            Type = a.Type,
                            StreetAddress = a.StreetAddress,
                            ApartmentSuite = a.ApartmentSuite,
                            City = a.City,
                            State = a.State,
                            PostalCode = a.PostalCode,
                            Country = a.Country,
                            IsDefault = a.IsDefault
                        }).ToList()
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Customer profile and addresses successfully updated.",
                        Type = ResponseTypes.Success
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException dex)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = dex.Message,
                        Type = ResponseTypes.Danger
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update customer.");
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Failed to update customer.",
                        Type = ResponseTypes.Danger
                    });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var query = new GetCustomerDetailsByIdQuery { Id = id };
                var customer = await _mediator.SendQueryAsync(query);

                if (customer == null) return NotFound();

                var details = new CustomerDetailsModel
                {
                    Id = customer.Id,
                    UserId = customer.UserId,
                    FullName = $"{customer.FirstName} {customer.LastName}",
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    Gender = customer.Gender ?? "Not Specified",
                    DateOfBirth = customer.DateOfBirth,
                    CreatedAt = customer.CreatedAt,
                    Origin = customer.Origin,
                    TotalOrders = customer.Orders.Count,
                    TotalSpent = customer.Orders.Sum(o => o.TotalAmount.Amount),
                    Addresses = customer.Addresses.Select(a => new CustomerAddressItemModel
                    {
                        Id = a.Id,
                        Type = a.Type.ToString(),
                        FullAddress = $"{a.Address.StreetAddress}, {a.Address.City} - {a.Address.PostalCode}, {a.Address.Country}",
                        IsDefault = a.IsDefault
                    }).ToList(),
                    RecentOrders = customer.Orders.OrderByDescending(o => o.OrderDate).Take(5).Select(o => new CustomerOrderItemModel
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        OrderDate = o.OrderDate,
                        Status = o.Status.ToString(),
                        PaymentStatus = o.PaymentStatus.ToString(),
                        TotalAmount = o.TotalAmount.Amount
                    }).ToList()
                };

                return View(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load customer profile.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedCustomers([FromBody] CustomerListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllCustomersByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("FirstName", "Email", "PhoneNumber", "Origin", "CreatedAt", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllCustomersByPagingQuery,
                    (System.Collections.Generic.IList<Domain.Entities.Customers.Customer>, int, int)>(query);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                HttpUtility.HtmlEncode($"{item.FirstName} {item.LastName}"),
                                HttpUtility.HtmlEncode(item.Email),
                                HttpUtility.HtmlEncode(item.PhoneNumber),
                                item.Origin.ToString(), // "Self", "Admin", or "Guest"
                                item.Orders?.Count.ToString() ?? "0",
                                item.CreatedAt.ToString("MMM dd, yyyy"),
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get customer list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleteCommand = new CustomerDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(deleteCommand);

                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Customer successfully deleted.",
                    Type = ResponseTypes.Success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer.");
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Failed to delete customer.",
                    Type = ResponseTypes.Danger
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}