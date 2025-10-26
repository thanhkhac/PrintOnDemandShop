using CleanArchitectureBase.Application.ProductFeedbacks;
using CleanArchitectureBase.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class FeedbackEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        
        // ✅ API tạo feedback cho đơn hàng
        group.MapPost("/", CreateFeedback)
            .WithName("CreateProductFeedback")
            .WithSummary("Tạo đánh giá sản phẩm sau khi mua")
            .WithDescription("Người dùng gửi feedback và rating cho các sản phẩm trong đơn hàng đã ở trạng thái  CONFIRM_RECEIVED ");
    }

    private async Task<IResult> CreateFeedback(
        ISender sender,
        [FromBody] CreateProductFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToOk();
    }
}
