using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Templates.Commands;
using CleanArchitectureBase.Application.Templates.Dtos;
using CleanArchitectureBase.Application.Templates.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class TemplateEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrUpdateTemplate, "/CreateOrUpdateTemplate");
        group.MapGet(SearchTemplates, "/Search");
        group.MapGet(GetTemplateDetail, "/{templateId:guid}");
        group.MapGet(GetTemplatesByProduct, "/Product/{productId:guid}");
        group.MapDelete(DeleteTemplate, "/{templateId:guid}");
    }

    /// <summary>
    /// Tạo hoặc update template
    /// </summary>
    public async Task<Ok<ApiResponse<Guid>>> CreateOrUpdateTemplate(
        [FromBody] CreateOrUpdateTemplateCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// Tìm kiếm templates với phân trang
    /// </summary>
    public async Task<Ok<ApiResponse<PaginatedList<TemplateDto>>>> SearchTemplates(
        [AsParameters] SearchTemplatesQuery query,
        ISender sender)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy chi tiết template
    /// </summary>
    public async Task<Ok<ApiResponse<TemplateDetailDto>>> GetTemplateDetail(
        [FromRoute] Guid templateId,
        ISender sender)
    {
        var query = new GetTemplateDetailQuery
        {
            TemplateId = templateId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Lấy tất cả templates của một sản phẩm
    /// </summary>
    public async Task<Ok<ApiResponse<List<TemplateDto>>>> GetTemplatesByProduct(
        [FromRoute] Guid productId,
        [FromQuery] Guid? productOptionValueId,
        ISender sender)
    {
        var query = new GetTemplatesByProductQuery
        {
            ProductId = productId,
            ProductOptionValueId = productOptionValueId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Xóa template
    /// </summary>
    public async Task<Ok<ApiResponse<bool>>> DeleteTemplate(
        [FromRoute] Guid templateId,
        ISender sender)
    {
        var command = new DeleteTemplateCommand { TemplateId = templateId };
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
