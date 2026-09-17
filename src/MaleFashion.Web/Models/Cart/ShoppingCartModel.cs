using MaleFashion.Application.Features.Carts.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Cart
{
    public class ShoppingCartModel
    {
        public CustomerCartDto Cart { get; set; } = new();
        public string? CouponErrorMessage { get; set; }
        public string? CouponSuccessMessage { get; set; }
    }
}

