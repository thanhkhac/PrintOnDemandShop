using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public RejectOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        if (order.Status == "Cancelled")
        {
            throw new BadRequestException("Order is already cancelled");
        }

        if (order.Status == "Approved")
        {
            throw new BadRequestException("Cannot reject an approved order");
        }

        order.Status = "Rejected";
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
