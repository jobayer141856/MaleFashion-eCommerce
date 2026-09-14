using MaleFashion.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class CustomerAddressViewModel
    {
        public Guid? Id { get; set; }

        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Please select an address type.")]
        [Display(Name = "Address Type")]
        public AddressType Type { get; set; } = AddressType.Shipping;

        [Required(ErrorMessage = "Street address is required.")]
        [Display(Name = "Street Address")]
        [StringLength(200, ErrorMessage = "Street address cannot exceed 200 characters.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Display(Name = "Apartment, Suite, Unit, etc. (optional)")]
        [StringLength(100, ErrorMessage = "Apartment/Suite cannot exceed 100 characters.")]
        public string? ApartmentSuite { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State / District is required.")]
        [Display(Name = "State / District")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code is required.")]
        [Display(Name = "Postal Code")]
        [StringLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = "Bangladesh";

        [Display(Name = "Set as default delivery address")]
        public bool IsDefault { get; set; }

        public string FormattedAddress
        {
            get
            {
                var apt = !string.IsNullOrWhiteSpace(ApartmentSuite) ? $"{ApartmentSuite}, " : "";
                return $"{StreetAddress}, {apt}{City} - {PostalCode}, {State}, {Country}";
            }
        }
    }
}

