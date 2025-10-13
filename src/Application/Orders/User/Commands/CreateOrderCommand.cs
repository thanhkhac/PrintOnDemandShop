using System.Collections.Specialized;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;

namespace CleanArchitectureBase.Application.Orders.User.Commands;

// [Authorize]
public class CreateOrderCommand : IRequest<OrderDetailResponseDto>
{
    public string? RecipientPhone { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? PaymentMethod { get; set; }
    public List<CreateOrderItemRequestDto> OrderItems { get; set; } = new();
    public List<string> VoucherCodes { get; set; } = new();
    public bool IsCreated { get; set; }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private static readonly string[] AllowPaymentMethods =
    {
        "COD", "ONLINE_PAYMENT"
    };

    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.RecipientPhone).NotEmpty();
        RuleFor(x => x.RecipientName).NotEmpty();
        RuleFor(x => x.RecipientAddress).NotEmpty();
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(x => AllowPaymentMethods.Contains(x))
            .WithMessage("Only 'COD' and 'ONLINE_PAYMENT' are allowed as options.");
        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .ForEach(y =>
            {
                y.SetValidator(new CreateOrderRequestDtoValidator());
            });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDetailResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _dateTime;

    public CreateOrderCommandHandler(IApplicationDbContext context, TimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<OrderDetailResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Determine order status and payment status based on payment method
        string orderStatus;
        string paymentStatus;

        switch (request.PaymentMethod)
        {
            case "ONLINE_PAYMENT":
                orderStatus = nameof(OrderStatus.PENDING);
                paymentStatus = nameof(OrderPaymentStatus.ONLINE_PAYMENT_AWAITING);
                break;
            case "COD":
                orderStatus = nameof(OrderStatus.PENDING);
                paymentStatus = nameof(OrderPaymentStatus.COD);
                break;
            default:
                throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, request.PaymentMethod,
                    "Invalid payment method");
        }

        //Tạo order mới
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderDate = _dateTime.GetUtcNow(),
            Status = orderStatus,
            PaymentStatus = paymentStatus,
            RecipientName = request.RecipientName,
            RecipientPhone = request.RecipientPhone,
            RecipientAddress = request.RecipientAddress,
            PaymentMethod = request.PaymentMethod,
            PaymentCode = PaymentConst.OrderCodePrefix + Guid.NewGuid().ToString("N")[..30],
            SubTotal = 0,
            TotalAmount = 0,
            DiscountAmount = 0
        };

        var (items, subTotal, discountAmount, totalAmount, variantsToUpdate) = CreateProductItem(request.OrderItems, request.VoucherCodes, order.Id);

        order.SubTotal = subTotal;
        order.DiscountAmount = discountAmount;
        order.TotalAmount = totalAmount;
        order.Items = items;

        if (request.IsCreated)
        {
            // Trừ stock cho các ProductVariant
            foreach (var variant in variantsToUpdate)
            {
                _context.ProductVariants.Update(variant);
            }

            _context.Orders.Add(order);
            _context.OrderItems.AddRange(items);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(order);
    }


    private (List<OrderItem>, long, long, long, List<ProductVariant>) CreateProductItem(List<CreateOrderItemRequestDto> orderItems, List<string> vouchers,
        Guid orderId)
    {
        var items = new List<OrderItem>();
        long totalSubTotal = 0;
        long totalDiscountAmount = 0;
        long totalAmount = 0;

        var requestProductVariantIds = orderItems.Select(x => x.ProductVariantId!.Value).Distinct().ToList();

        var existingProductVariants = _context
            .ProductVariants
            .Include(x => x.Product)
            .Where(x => requestProductVariantIds.Contains(x.Id) && x.IsDeleted == false)
            .Where(x => x.Product!.IsDeleted == false)
            .ToList();
        // == Kiểm tra các sản phẩm còn tồn tại không

        var existingProductVariantIds = existingProductVariants.Select(x => x.Id).ToList();
        var invalidProductVariants = requestProductVariantIds.Except(existingProductVariantIds).ToList();


        if (invalidProductVariants.Any())
        {
            var message = $"Không tìm thấy các biến thể sản phẩm với id {string.Join(", ", invalidProductVariants)}";
            throw new ErrorCodeException(ErrorCodes.ORDER_VARIANTS_NOT_FOUND, invalidProductVariants, message);
        }

        // == Kiểm tra các sản phầm còn hàng không

        var groupRequestVariantIdsAndQuantity = orderItems
            .GroupBy(x => x.ProductVariantId)
            .ToDictionary(
                x => x.Key!.Value,
                x => x.Sum(x => x.Quantity)
            );

        var insufficientStockVariantIds = new List<Guid>();
        foreach (var epv in existingProductVariants)
        {
            var requestQuantity = groupRequestVariantIdsAndQuantity[epv.Id];
            if (requestQuantity > epv.Stock)
            {
                insufficientStockVariantIds.Add(epv.Id);
            }
        }

        if (insufficientStockVariantIds.Any())
        {
            var message = $"Không tìm thấy các biến thể sản phẩm với id {string.Join(", ", insufficientStockVariantIds)}";
            throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_STOCK, insufficientStockVariantIds, message);
        }

        // == Kiểm tra các voucher có tồn tại không
        var requestVoucherCodes = vouchers.Distinct().ToList();
        var existingVoucherEntities = _context.Vouchers
            .Include(x => x.ProductVouchers)
            .Where(x => requestVoucherCodes.Contains(x.Code))
            .ToList();

        if (existingVoucherEntities.Count != requestVoucherCodes.Count)
        {
            var missing = requestVoucherCodes.Except(existingVoucherEntities.Select(v => v.Code)).ToList();
            throw new ErrorCodeException(ErrorCodes.VOUCHER_NOT_FOUND, missing,
                $"Không tìm thấy các voucher: {string.Join(", ", missing)}");
        }

        //Kiểm tra các voucher còn thời hạn không
        var now = DateTime.UtcNow;
        var invalidVouchers = existingVoucherEntities
            .Where(v => v.StartDate > now || v.EndDate < now)
            .Select(v => v.Code)
            .ToList();

        if (invalidVouchers.Any())
        {
            throw new ErrorCodeException(ErrorCodes.VOUCHER_INVALID_DATE, invalidVouchers,
                $"Các voucher không hợp lệ (chưa bắt đầu hoặc đã hết hạn): {string.Join(", ", invalidVouchers)}");
        }

        //TODO: Cần bổ sung thêm check design cũng như, bản design đã khớp với variant chưa (Thông qua option value)
        var designIds = orderItems.Where(x => x.DesignId != null).Select(x => x.DesignId).Distinct().ToList();
        var existingDesign = _context.ProductDesigns
            // .Include(x => x.Product)
            .Where(x => designIds.Contains(x.Id)).ToList();

        if (designIds.Count != existingDesign.Count)
        {
            throw new ErrorCodeException(ErrorCodes.PRODUCT_DESIGN_NOT_FOUND);
        }

        // === Ghép sản phẩm với voucher hợp lệ theo thời gian
        var variantDict = existingProductVariants
            .ToDictionary(x => x.Id,
                x => new
                {
                    x.Product,
                    Variant = x
                });


        var validVouchers = existingVoucherEntities
            .Where(v => v.StartDate <= now && now <= v.EndDate)
            .ToList();

        var productToVouchers = validVouchers
            .SelectMany(v => v.ProductVouchers.Select(pv => new
            {
                pv.ProductId,
                Voucher = v
            }))
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Voucher).ToList());

        // === Áp dụng voucher và tính giá

        // Track các variant cần update stock
        var variantsToUpdate = new List<ProductVariant>();

        //Lọc các sản phẩm trong order item
        foreach (var item in orderItems)
        {
            //Biến thể sản phẩm hiện tại
            var variant = existingProductVariants.First(x => x.Id == item.ProductVariantId);
            // Id của sản phẩm
            var productId = variant.ProductId;
            //Số lượng cần mua
            var quantity = item.Quantity;
            //Giá của biến thể sản phẩm
            var unitPrice = variant.UnitPrice;
            //Tổng tiền (chưa discount)
            var totalBeforeDiscount = quantity * unitPrice;

            // Trừ stock cho variant
            variant.Stock -= quantity;
            variantsToUpdate.Add(variant);

            long finalUnitPrice = unitPrice;
            long discountPerUnit = 0;
            Voucher? appliedVoucher = null;

            long? voucherDiscountAmount = null;
            long? voucherDiscountPercent = null;

            if (productToVouchers.TryGetValue(productId, out var vouchersForProduct))
            {
                foreach (var voucher in vouchersForProduct)
                {
                    if (voucher.UsedCount >= voucher.UsageLimit)
                        continue;

                    long discount = 0;
                    var discountValue = voucher.DiscountValue ?? 0;

                    if (voucher.DiscountType == "PERCENT")
                    {
                        discount = (long)Math.Round(unitPrice * (discountValue / 100m));
                        voucherDiscountPercent = discount;
                    }
                    else if (voucher.DiscountType == "FIXED_AMOUNT")
                    {
                        discount = discountValue;
                        voucherDiscountAmount = discountValue;
                    }

                    var discountedPrice = unitPrice - discount;
                    if (discountedPrice < 0) discountedPrice = 0;
                    if (discountedPrice < finalUnitPrice)
                    {
                        finalUnitPrice = discountedPrice;
                        discountPerUnit = discount;
                        appliedVoucher = voucher;
                    }
                }
            }

            var totalDiscount = discountPerUnit * quantity;
            var totalAfterDiscount = totalBeforeDiscount - totalDiscount;


            Console.WriteLine(
                $"Variant {variant.Id} | Design {item.DesignId} | {quantity} x {unitPrice} = {totalAfterDiscount} (Giảm {totalDiscount})"
            );

            var newOrderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductVariantId = item.ProductVariantId!.Value,
                ProductDesignId = item.DesignId,
                VoucherId = appliedVoucher?.Id,
                Name = variantDict[item.ProductVariantId.Value]!.Product?.Name ?? string.Empty,
                VariantSku = variantDict[item.ProductVariantId.Value]!.Variant?.Sku ?? string.Empty,
                VariantImageUrl = variantDict[item.ProductVariantId.Value]!.Product?.ImageUrl,
                UnitPrice = variant.UnitPrice,
                Quantity = item.Quantity,
                SubTotal = totalBeforeDiscount,
                DiscountAmount = totalDiscount,
                TotalAmount = totalAfterDiscount,
                VoucherCode = appliedVoucher?.Code,
                VoucherDiscountAmount = voucherDiscountAmount,
                VoucherDiscountPercent = voucherDiscountPercent
            };

            items.Add(newOrderItem);

            // Cộng dồn các thông số
            totalSubTotal += totalBeforeDiscount;
            totalDiscountAmount += totalDiscount;
            totalAmount += totalAfterDiscount;

            if (appliedVoucher != null)
            {
                var voucher = validVouchers.First(v => v.Id == appliedVoucher.Id);
                voucher.UsedCount += 1;
            }
        }

        return (items, totalSubTotal, totalDiscountAmount, totalAmount, variantsToUpdate);
    }

    private static OrderDetailResponseDto MapToDto(Order order)
    {
        return new OrderDetailResponseDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            RecipientName = order.RecipientName,
            RecipientPhone = order.RecipientPhone,
            RecipientAddress = order.RecipientAddress,
            PaymentMethod = order.PaymentMethod,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            UserFeedback = order.UserFeedback,
            Rating = order.Rating, // Giữ nguyên nullable
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductDesignId = i.ProductDesignId,
                VoucherId = i.VoucherId,
                Name = i.Name,
                VariantSku = i.VariantSku,
                ImageUrl = i.VariantImageUrl,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TotalAmount = i.TotalAmount,
                VoucherCode = i.VoucherCode,
                VoucherDiscountAmount = i.VoucherDiscountAmount,
                VoucherDiscountPercent = i.VoucherDiscountPercent
            }).ToList()
        };
    }
}
