using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Checkout
{
    public class CheckoutViewModel
    {
        // Customer Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Shipping Address
        [Required(ErrorMessage = "Street address is required")]
        [Display(Name = "Street Address")]
        public string ShippingStreetAddress { get; set; } = string.Empty;

        [Display(Name = "Apartment / Suite")]
        public string? ShippingApartmentSuite { get; set; }

        [Required(ErrorMessage = "City / Town is required")]
        [Display(Name = "Town / City")]
        public string ShippingCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "District / State is required")]
        [Display(Name = "District / State")]
        public string ShippingState { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postcode / ZIP is required")]
        [Display(Name = "Postcode / ZIP")]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "Bangladesh";

        // Separate Billing Address Option
        public bool ShipToDifferentAddress { get; set; } = false;

        public string? BillingStreetAddress { get; set; }
        public string? BillingApartmentSuite { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; } = "Bangladesh";

        // Order Notes
        [Display(Name = "Order notes")]
        public string? OrderNotes { get; set; }

        // Payment Method
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.SSLCommerz;

        // Cart Data (for summary display)
        public CustomerCartDto Cart { get; set; } = new();

        public decimal ShippingFee => Cart.Subtotal >= 1500m ? 0m : 60m;
        public decimal FinalTotal => Math.Max(0, Cart.Subtotal + ShippingFee - Cart.DiscountAmount);

        // Saved Addresses for Logged-In Users
        public List<CustomerAddress>? SavedAddresses { get; set; }
        public Guid? SelectedSavedAddressId { get; set; }
    }
}

