using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Vouchers.Commands;
using CleanArchitectureBase.Application.Vouchers.Dtos;
using CleanArchitectureBase.Application.Vouchers.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class VoucherEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        // Voucher management
        group.MapPost(CreateOrUpdateVoucher, "/CreateOrUpdateVoucher");
        group.MapGet(SearchVouchers, "/Search");
        group.MapGet(GetVoucherDetail, "/{voucherId:guid}");
        group.MapDelete(DeleteVoucher, "/{voucherId:guid}");
        
        // Product-Voucher management
        group.MapPost(AddVoucherToProduct, "/AddToProduct");
        group.MapPost(RemoveVoucherFromProduct, "/RemoveFromProduct");
        group.MapGet(GetVouchersByProduct, "/Product/{productId:guid}");
    }

    /// <summary>
    /// Tạo hoặc update voucher (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<Guid>>> CreateOrUpdateVoucher(
        [FromBody] CreateOrUpdateVoucherCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Tìm kiếm vouchers với phân trang (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<PaginatedList<VoucherDto>>>> SearchVouchers(
        [AsParameters] SearchVouchersQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy chi tiết voucher (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<VoucherDetailDto>>> GetVoucherDetail(
        [FromRoute] Guid voucherId,
        ISender sender)
    {
        var query = new GetVoucherDetailQuery
        {
            VoucherId = voucherId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Xóa voucher (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<bool>>> DeleteVoucher(
        [FromRoute] Guid voucherId,
        ISender sender)
    {
        var command = new DeleteVoucherCommand { VoucherId = voucherId };
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Thêm voucher vào products (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<bool>>> AddVoucherToProduct(
        [FromBody] AddVoucherToProductCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Xóa voucher khỏi products (Admin/Moderator only)
    /// </summary>
    public async Task<Ok<ApiResponse<bool>>> RemoveVoucherFromProduct(
        [FromBody] RemoveVoucherFromProductCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy tất cả vouchers áp dụng cho một product
    /// </summary>
    public async Task<Ok<ApiResponse<List<VoucherDto>>>> GetVouchersByProduct(
        [FromRoute] Guid productId,
        [FromQuery] bool? isActive,
        ISender sender)
    {
        var query = new GetVouchersByProductQuery
        {
            ProductId = productId,
            IsActive = isActive
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }
}
