using System;

namespace MaleFashion.Web.Models.Cart
{
    public class CartSummaryModel
    {
        public int TotalItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "BDT";
    }
}

