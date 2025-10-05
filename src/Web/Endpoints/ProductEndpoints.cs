using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Payments.Queries;
using CleanArchitectureBase.Application.Products.Commands;
using CleanArchitectureBase.Application.Products.Dtos.ResponseDtos;
using CleanArchitectureBase.Application.Products.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class ProductEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrUpdateProduct, "/CreateOrUpdateProduct");
        group.MapGet(SearchProducts, "/Search");
        group.MapGet(GetProductDetail, "/{productId:guid}");
        group.MapDelete(DeleteProduct, "/{productId:guid}");
    }
    
    /// <summary>
    /// Tạo hoặc update sản phẩm. <br/>
    /// Các <c>Options.Name</c> cho phép là: <br/>
    /// - SIZE <br/>
    /// - COLOR
    /// </summary>
    public async Task<Ok<ApiResponse<Guid>>> CreateOrUpdateProduct(
        [FromBody] CreateUpdateProductCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<PaginatedList<ProductForSearchResponseDto>>>> SearchProducts(
        [AsParameters] SearchProductQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<ProductDetailResponseDto>>> GetProductDetail(
        [FromRoute]Guid productId,
        ISender sender)
    {
        var query = new GetProductDetailQuery
        {
            ProductId = productId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<bool>>> DeleteProduct(
        Guid productId,
        ISender sender)
    {
        var command = new DeleteProductCommand { ProductId = productId };
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
