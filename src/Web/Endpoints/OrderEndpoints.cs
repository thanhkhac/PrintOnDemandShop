using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Application.Orders.User.Commands;
using CleanArchitectureBase.Application.Orders.User.Queries;
using CleanArchitectureBase.Application.Orders.Admin.Queries;
using CleanArchitectureBase.Application.Orders.Admin.Commands;
using CleanArchitectureBase.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static CleanArchitectureBase.Application.Orders.User.Commands.UpdateMyOrderStatusCommand;

namespace CleanArchitectureBase.Web.Endpoints;

public class OrderEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        // User endpoints
        group.MapPost(CreateOrder);
        group.MapGet(GetMyOrders);
        group.MapGet("{orderId:guid}", GetOrderDetail);
        group.MapPut("{orderId:guid}/status", UpdateMyOrderStatus);
        
        // Admin endpoints
        group.MapGet("admin/all", GetAllOrders);
        group.MapGet("admin/{orderId:guid}", GetAdminOrderDetail);
        group.MapPut("admin/{orderId:guid}/status", UpdateOrderStatus);
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

    public async Task<Ok<ApiResponse<string>>> UpdateMyOrderStatus(
        Guid orderId,
        [FromBody] UpdateMyOrderStatusRequest request,
        ISender sender)
    {
        var command = new UpdateMyOrderStatusCommand 
        { 
            OrderId = orderId,
            Action = request.Action,
            Feedback = request.Feedback,
            Rating = request.Rating
        };
        await sender.Send(command);
        
        string message = request.Action switch
        {
            UserOrderAction.CANCEL => "Order cancelled successfully",
            UserOrderAction.CONFIRM_RECEIVED => "Order confirmed as received successfully",
            _ => "Order updated successfully"
        };
        
        return ApiResponse.Success(message).ToOk();
    }

    // Admin methods
    public async Task<Ok<ApiResponse<PaginatedList<OrderDetailResponseDto>>>> GetAllOrders(
        [AsParameters] GetAllOrdersQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<OrderDetailResponseDto>>> GetAdminOrderDetail(
        Guid orderId,
        ISender sender)
    {
        var query = new GetOrderDetailQuery { OrderId = orderId };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<string>>> UpdateOrderStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        ISender sender)
    {
        var command = new UpdateOrderStatusCommand 
        { 
            OrderId = orderId,
            Status = request.Status,
            Notes = request.Notes
        };
        await sender.Send(command);
        return ApiResponse.Success("Order status updated successfully").ToOk();
    }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMyOrderStatusRequest
{
    public UserOrderAction Action { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
}
