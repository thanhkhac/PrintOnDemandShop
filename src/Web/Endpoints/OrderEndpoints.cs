using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Application.Orders.User.Commands;
using CleanArchitectureBase.Application.Orders.User.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class OrderEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrder);
        group.MapGet(GetMyOrders);
        group.MapGet("{orderId:guid}", GetOrderDetail);
    }

    public async Task<Ok<ApiResponse<OrderDetailResponseDto>>> CreateOrder(
        [FromBody] CreateOrderCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<PaginatedList<OrderDetailResponseDto>>>> GetMyOrders(
        [AsParameters] GetMyOrdersQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<OrderDetailResponseDto>>> GetOrderDetail(
        Guid orderId,
        ISender sender)
    {
        var query = new GetOrderDetailQuery { OrderId = orderId };
        var result = await sender.Send(query);
        return result.ToOk();
    }
}
