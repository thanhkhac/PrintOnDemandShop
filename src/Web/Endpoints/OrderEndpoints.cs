using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Application.Orders.User.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class OrderEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrder);
    }

    public async Task<Ok<ApiResponse<OrderDetailResponseDto>>> CreateOrder(
        [FromBody] CreateOrderCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
