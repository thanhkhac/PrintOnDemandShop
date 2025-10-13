using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

[Authorize(Roles = "Admin")]
public class RejectOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
    public string? RejectReason { get; set; }
}
