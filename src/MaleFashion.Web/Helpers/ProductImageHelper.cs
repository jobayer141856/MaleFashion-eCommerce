using System;

namespace MaleFashion.Web.Helpers
{
    public static class ProductImageHelper
    {
        public const string DefaultPlaceholder = "/malefashion/img/product/product-1.jpg";

        public static string GetProductImageUrl(string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return DefaultPlaceholder;

            rawUrl = rawUrl.Trim();

            if (rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return rawUrl;
            }

            if (rawUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                rawUrl.StartsWith("/malefashion/", StringComparison.OrdinalIgnoreCase) ||
                rawUrl.StartsWith("/img/", StringComparison.OrdinalIgnoreCase))
            {
                return rawUrl;
            }

            if (rawUrl.StartsWith("/"))
            {
                return rawUrl;
            }

            // Uploaded product images stored by filename
            return $"/uploads/products/{rawUrl}";
        }
    }
}

