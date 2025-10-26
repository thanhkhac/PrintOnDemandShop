using CleanArchitectureBase.Application.TokenPackages.Commands;
using CleanArchitectureBase.Application.TokenPackages.Queries;
using CleanArchitectureBase.Application.TokenPackages.Queries.Admin;
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

        // 🟩 3. Admin – Lấy lịch sử các gói token đã thanh toán
        group.MapGet("/Admin/History", AdminGetHistory)
            .WithName("AdminGetPaidTokenPackages")
            .WithSummary("Admin xem danh sách các token package đã thanh toán")
            .WithDescription("Bao gồm thông tin người mua, giá, token nhận được, và thời gian mua");
    }

    /// <summary>
    /// 🟢 Người dùng mua gói token (tạo lệnh thanh toán)
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
    /// 🟢 Kiểm tra trạng thái thanh toán của token package
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

    /// <summary>
    /// 🔵 Admin xem lịch sử token package đã thanh toán
    /// </summary>
    private async Task<IResult> AdminGetHistory(
        ISender sender,
        [AsParameters] AdminGetTokenPackageHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToOk();
    }
}
