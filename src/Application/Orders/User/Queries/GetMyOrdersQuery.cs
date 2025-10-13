using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;

namespace CleanArchitectureBase.Application.Orders.User.Queries;
[Authorize]
public class GetMyOrdersQuery : PaginatedQuery,IRequest<PaginatedList<OrderDetailResponseDto>>
{
    public string? Status { get; set; }
}
