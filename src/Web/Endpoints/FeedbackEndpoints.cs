using CleanArchitectureBase.Application.ProductFeedbacks;
using CleanArchitectureBase.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
            .WithDescription("Người dùng gửi feedback và rating cho các sản phẩm trong đơn hàng đã ở trạng thái CONFIRM_RECEIVED.");

        // ✅ API lấy feedback theo ProductId
        group.MapGet("/Product/{productId:guid}", GetProductFeedback)
            .WithName("GetProductFeedback")
            .WithSummary("Lấy danh sách đánh giá cho sản phẩm")
            .WithDescription("Trả về danh sách các đánh giá (feedbacks) và điểm trung bình rating của sản phẩm.");
    }

    private async Task<Ok<ApiResponse<Guid>>> CreateFeedback(
        ISender sender,
        [FromBody] CreateProductFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToOk();
    }

    private async Task<Ok<ApiResponse<ProductFeedbackListDto>>> GetProductFeedback(
        ISender sender,
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductFeedbackQuery { ProductId = productId };
        var result = await sender.Send(query, cancellationToken);
        return result.ToOk();
    }
}
