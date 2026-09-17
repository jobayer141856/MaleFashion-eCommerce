using System.Collections.Generic;

namespace MaleFashion.Application.Features.Wishlists.DTOs
{
    public class CustomerWishlistDto
    {
        public List<CustomerWishlistItemDto> Items { get; set; } = new();
        public int TotalCount => Items?.Count ?? 0;
        public string Currency { get; set; } = "BDT";
    }
}

