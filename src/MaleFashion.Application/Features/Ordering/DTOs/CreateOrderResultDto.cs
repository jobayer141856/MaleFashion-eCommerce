using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Ordering.DTOs
{
    public class CreateOrderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}

