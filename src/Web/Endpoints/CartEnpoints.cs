using CleanArchitectureBase.Application.Carts;
using CleanArchitectureBase.Application.Carts.Commands;
using CleanArchitectureBase.Application.Carts.Dtos.Response;
using CleanArchitectureBase.Application.Carts.Queries;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class CartEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(AddToCart, "/item");
        group.MapDelete(RemoveFromCart, "/item");
        group.MapGet(GetCartItems, "/item");
    }

    public async Task<Ok<ApiResponse<Guid>>> AddToCart(
        [FromBody] AddToCartCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<bool>>> RemoveFromCart(
        [FromBody] RemoveFromCartCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<List<CartItemResponseDto>>>> GetCartItems(
        ISender sender)
    {
        var query = new GetCartItemsQuery();
        var result = await sender.Send(query);
        return result.ToOk();
    }
}
