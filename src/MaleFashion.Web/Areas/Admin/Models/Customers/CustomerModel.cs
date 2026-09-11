using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Customers
{
    public class CustomerAddressModel
    {
        public Guid? Id { get; set; }
        public AddressType Type { get; set; } = AddressType.Shipping;

        [Required(ErrorMessage = "Street address is required.")]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; } = string.Empty;

        [Display(Name = "Apt / Suite")]
        public string? ApartmentSuite { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = "Bangladesh";
        public bool IsDefault { get; set; }
    }

    public class CustomerModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [StringLength(30)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Account Password")]
        public string? Password { get; set; } = "Customer@123";

        // Initial creation flat address fields
        public string? StreetAddress { get; set; }
        public string? ApartmentSuite { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; } = "Bangladesh";
        public List<CustomerAddressModel> Addresses { get; set; } = new();
    }
}