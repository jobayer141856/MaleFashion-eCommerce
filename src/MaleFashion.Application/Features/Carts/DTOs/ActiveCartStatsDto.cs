using System;

namespace MaleFashion.Application.Features.Carts.DTOs
{
    public class ActiveCartStatsDto
    {
        public int TotalActiveCarts { get; set; }
        public decimal PotentialRevenue { get; set; }
        public int RegisteredCartsCount { get; set; }
        public int GuestCartsCount { get; set; }
        public int ActiveRecentCount { get; set; }
        public int IdleCount { get; set; }
        public int AbandonedCount { get; set; }
        public decimal RegisteredRevenue { get; set; }
        public decimal GuestRevenue { get; set; }
    }
}

