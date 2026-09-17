using MaleFashion.Application.Features.Wishlists.DTOs;

namespace MaleFashion.Web.Models.Wishlist
{
    public class WishlistIndexModel
    {
        public CustomerWishlistDto Wishlist { get; set; } = new();
        public string Currency { get; set; } = "BDT";
    }
}

