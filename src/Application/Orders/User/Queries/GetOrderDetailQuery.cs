using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;

namespace CleanArchitectureBase.Application.Orders.User.Queries;

[Authorize]
public class GetOrderDetailQuery : IRequest<OrderDetailResponseDto>
{
    public Guid OrderId { get; set; }
}
