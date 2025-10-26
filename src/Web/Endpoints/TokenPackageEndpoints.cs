using CleanArchitectureBase.Application.TokenPackages.Commands;
using CleanArchitectureBase.Application.TokenPackages.Queries;
using CleanArchitectureBase.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class TokenPackageEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        // 🟩 1. Tạo đơn hàng mua token package
        group.MapPost("/Buy", Buy)
            .WithName("BuyTokenPackage")
            .WithSummary("Tạo lệnh thanh toán token package")
            .WithDescription("Người dùng chọn gói token, hệ thống tạo PaymentCode và hạn thanh toán (3 phút)");

        // 🟩 2. Kiểm tra đơn token package đã thanh toán chưa
        group.MapGet("/CheckIsPaid", CheckIsPaid)
            .WithName("CheckTokenPackageIsPaid")
            .WithSummary("Kiểm tra trạng thái thanh toán của token package")
            .WithDescription("Trả về true nếu đã thanh toán, false nếu chưa hoặc hết hạn");

    }

    /// <summary>
    /// Mua gói token (tạo lệnh thanh toán)
    /// </summary>
    private async Task<IResult> Buy(
        ISender sender,
        [FromBody] CreateTokenPackageOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToOk();
    }

    /// <summary>
    /// Kiểm tra trạng thái thanh toán
    /// </summary>
    private async Task<IResult> CheckIsPaid(
        ISender sender,
        [FromQuery] string paymentCode,
        CancellationToken cancellationToken)
    {
        var query = new CheckTokenPackageIsPaidRequest { PaymentCode = paymentCode };
        var result = await sender.Send(query, cancellationToken);
        return result.ToOk();
    }
}
