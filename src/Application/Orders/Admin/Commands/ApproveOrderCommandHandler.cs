using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

public class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public ApproveOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        if (order.Status == "Approved")
        {
            throw new BadRequestException("Order is already approved");
        }

        if (order.Status == "Cancelled")
        {
            throw new BadRequestException("Cannot approve a cancelled order");
        }

        order.Status = "Approved";
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
