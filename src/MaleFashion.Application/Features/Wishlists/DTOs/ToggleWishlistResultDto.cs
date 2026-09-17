namespace MaleFashion.Application.Features.Wishlists.DTOs
{
    public class ToggleWishlistResultDto
    {
        public bool Success { get; set; }
        public bool IsAdded { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}

