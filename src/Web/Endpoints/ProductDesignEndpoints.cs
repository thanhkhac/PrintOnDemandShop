using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.ProductDesigns.Commands;
using CleanArchitectureBase.Application.ProductDesigns.Dtos;
using CleanArchitectureBase.Application.ProductDesigns.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class ProductDesignEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrUpdateProductDesign, "/CreateOrUpdateProductDesign");
        group.MapGet(SearchProductDesigns, "/Search");
        group.MapGet(GetProductDesignDetail, "/{productDesignId:guid}");
        group.MapGet(GetProductDesignsByProduct, "/Product/{productId:guid}");
        group.MapDelete(DeleteProductDesign, "/{productDesignId:guid}");
    }

    /// <summary>
    /// Tạo hoặc update thiết kế sản phẩm của người dùng
    /// </summary>
    public async Task<Ok<ApiResponse<Guid>>> CreateOrUpdateProductDesign(
        [FromBody] CreateOrUpdateProductDesignCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Tìm kiếm thiết kế sản phẩm của người dùng hiện tại với phân trang
    /// </summary>
    public async Task<Ok<ApiResponse<PaginatedList<ProductDesignDto>>>> SearchProductDesigns(
        [AsParameters] SearchProductDesignsQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy chi tiết thiết kế sản phẩm
    /// </summary>
    public async Task<Ok<ApiResponse<ProductDesignDetailDto>>> GetProductDesignDetail(
        [FromRoute] Guid productDesignId,
        ISender sender)
    {
        var query = new GetProductDesignDetailQuery
        {
            ProductDesignId = productDesignId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy tất cả thiết kế của người dùng cho một sản phẩm cụ thể
    /// </summary>
    public async Task<Ok<ApiResponse<List<ProductDesignDto>>>> GetProductDesignsByProduct(
        [FromRoute] Guid productId,
        [FromQuery] Guid? productOptionValueId,
        ISender sender)
    {
        var query = new GetProductDesignsByProductQuery
        {
            ProductId = productId,
            ProductOptionValueId = productOptionValueId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Xóa thiết kế sản phẩm của người dùng
    /// </summary>
    public async Task<Ok<ApiResponse<bool>>> DeleteProductDesign(
        [FromRoute] Guid productDesignId,
        ISender sender)
    {
        var command = new DeleteProductDesignCommand { ProductDesignId = productDesignId };
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
