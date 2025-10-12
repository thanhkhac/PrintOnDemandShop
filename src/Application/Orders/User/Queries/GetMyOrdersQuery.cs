using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;

namespace CleanArchitectureBase.Application.Orders.User.Queries;
[Authorize]
public class GetMyOrdersQuery : IRequest<PaginatedList<OrderDetailResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
}
