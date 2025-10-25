using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;

namespace CleanArchitectureBase.Application.ProductFeedbacks;

[Authorize]
public class CreateProductFeedbackCommand : IRequest<Guid>
{
    public Guid OrderId { get; set; }
    public string? Feedback { get; set; }
    public int Rating { get; set; }
}

public class CreateProductFeedbackCommandValidator : AbstractValidator<CreateProductFeedbackCommand>
{
    public CreateProductFeedbackCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Feedback).NotEmpty();
        RuleFor(x => x.Rating).GreaterThan(0);
    }
}

public class CreateProductFeedbackCommandHandler : IRequestHandler<CreateProductFeedbackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public CreateProductFeedbackCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }


    public async Task<Guid> Handle(CreateProductFeedbackCommand request, CancellationToken cancellationToken)
    {
        var order = _context
            .Orders.Include(x => x.Items).ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product).FirstOrDefault(x => x.Id == request.OrderId);

        if (order == null)
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND);

        if (order.CreatedBy != _user.UserId)
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN);
            
        // if(order.Status != nameof(OrderStatus.CONFIRM_RECEIVED))
        //     throw new ErrorCodeException()

        var products = order.Items
            .Select(i => i.ProductVariant!.Product!)
            .Distinct()
            .ToList();

        foreach (var product in products)
        {
            var feedback = new ProductFeedback
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                OrderId = order.Id,
                Feedback = request.Feedback,
                Rating = request.Rating,
                CreatedBy = _user.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProductFeedbacks.Add(feedback);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
