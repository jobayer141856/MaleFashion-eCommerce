using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Promotions;
using System;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownAddCommand : ICommand<DealCountdown>
    {
        public Guid ProductId { get; set; }
        public string DealTitle { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public string Currency { get; set; } = "BDT";
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

