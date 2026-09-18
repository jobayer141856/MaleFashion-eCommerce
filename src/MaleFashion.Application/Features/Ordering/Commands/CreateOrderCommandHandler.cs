using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResultDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateOrderResultDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            // 1. Retrieve Active Cart
            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return new CreateOrderResultDto
                {
                    Success = false,
                    Message = "Your shopping cart is empty."
                };
            }

            // 2. Validate Variant Stock & Deduct
            foreach (var item in cart.Items)
            {
                if (item.ProductVariant != null)
                {
                    if (item.ProductVariant.StockQuantity < item.Quantity)
                    {
                        var variantDesc = $"{item.ProductVariant.Size} / {item.ProductVariant.ColorName}".Trim(' ', '/');
                        return new CreateOrderResultDto
                        {
                            Success = false,
                            Message = $"Insufficient stock for {item.ProductName} ({variantDesc}). Available: {item.ProductVariant.StockQuantity}, In Cart: {item.Quantity}."
                        };
                    }

                    // Deduct stock quantity
                    item.ProductVariant.StockQuantity -= item.Quantity;
                }
            }

            // 3. Subtotal calculation
            decimal subtotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice.Amount);

            // 4. Shipping calculation: Free over BDT 1500, else flat fee BDT 60
            decimal shippingCost = subtotal >= 1500m ? 0m : 60m;

            // 5. Coupon discount calculation
            decimal discountAmount = 0m;
            Guid? couponId = null;

            if (!string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
            {
                var now = DateTime.UtcNow;
                var coupons = await _unitOfWork.CouponRepository.GetAllAsync(cancellationToken);
                var coupon = coupons.FirstOrDefault(c =>
                    string.Equals(c.Code, cart.AppliedCouponCode, StringComparison.OrdinalIgnoreCase) &&
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate <= now) &&
                    c.ExpirationDate >= now);

                if (coupon != null && (!coupon.MinimumSpend.HasValue || subtotal >= coupon.MinimumSpend.Value))
                {
                    if (coupon.DiscountType == DiscountType.Percentage)
                    {
                        discountAmount = Math.Round((subtotal * coupon.DiscountValue) / 100m, 2);
                    }
                    else
                    {
                        discountAmount = Math.Min(coupon.DiscountValue, subtotal);
                    }

                    coupon.UsedCount++;
                    couponId = coupon.Id;
                }
            }

            decimal totalAmount = Math.Max(0, subtotal + shippingCost - discountAmount);

            // 6. Customer Identification (Self vs Guest)
            Guid? customerId = command.CustomerId;
            if (!customerId.HasValue && !string.IsNullOrWhiteSpace(command.Email))
            {
                var allCustomers = await _unitOfWork.CustomerRepository.GetAllAsync(cancellationToken);
                var existingCustomer = allCustomers.FirstOrDefault(c =>
                    string.Equals(c.Email, command.Email.Trim(), StringComparison.OrdinalIgnoreCase));

                if (existingCustomer != null)
                {
                    customerId = existingCustomer.Id;
                }
                else
                {
                    var guestCustomer = new Customer
                    {
                        Id = IdentityGenerator.NewSequentialGuid(),
                        FirstName = command.FirstName.Trim(),
                        LastName = command.LastName.Trim(),
                        Email = command.Email.Trim(),
                        PhoneNumber = command.PhoneNumber.Trim(),
                        Origin = CustomerOrigin.Guest,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.CustomerRepository.AddAsync(guestCustomer, cancellationToken);
                    customerId = guestCustomer.Id;
                }
            }

            // 7. Generate Order Number
            string orderNumber = $"MF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            // 8. Create Order Entity
            var order = new Order
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                OrderNumber = orderNumber,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = command.PaymentMethod,
                ShippingAddress = command.ShippingAddress,
                BillingAddress = command.BillingAddress != null && !ReferenceEquals(command.BillingAddress, command.ShippingAddress)
                    ? command.BillingAddress
                    : (command.ShippingAddress with { }),
                Subtotal = new Money(subtotal, "BDT"),
                ShippingCost = new Money(shippingCost, "BDT"),
                DiscountAmount = new Money(discountAmount, "BDT"),
                TotalAmount = new Money(totalAmount, "BDT"),
                OrderNotes = command.OrderNotes,
                CouponId = couponId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = IdentityGenerator.NewSequentialGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    UnitPrice = new Money(item.UnitPrice.Amount, "BDT"),
                    Quantity = item.Quantity,
                    TotalPrice = new Money(item.Quantity * item.UnitPrice.Amount, "BDT"),
                    CreatedAt = DateTime.UtcNow
                };
                order.OrderItems.Add(orderItem);
            }

            await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);

            // 9. Clear Cart
            cart.Items.Clear();
            cart.AppliedCouponCode = null;
            cart.UpdatedAt = DateTime.UtcNow;

            // 10. Commit changes
            await _unitOfWork.SaveAsync(cancellationToken);

            return new CreateOrderResultDto
            {
                Success = true,
                Message = "Order created successfully.",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = totalAmount,
                PaymentMethod = order.PaymentMethod
            };
        }
    }
}

