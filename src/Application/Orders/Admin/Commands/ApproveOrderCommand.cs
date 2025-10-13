using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

[Authorize(Roles = "Admin")]
public class ApproveOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
    public string? Notes { get; set; }
}
