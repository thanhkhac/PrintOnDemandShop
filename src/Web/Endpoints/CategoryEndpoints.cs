using CleanArchitectureBase.Application.Categories.Commands;
using CleanArchitectureBase.Application.Categories.Dtos;
using CleanArchitectureBase.Application.Categories.Queries;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class CategoryEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(CreateOrUpdateCategory, "CreateOrUpdate");
        group.MapGet(GetAllCategories, "GetAll");
        group.MapGet(GetCategoryById, "{id:guid}");
        group.MapDelete(DeleteCategory, "{id:guid}");
    }
    
    
    public async Task<Ok<ApiResponse<CategoryDto>>> CreateOrUpdateCategory([FromBody] CreateUpdateCategoryCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }
    
    
    public async Task<Ok<ApiResponse>> DeleteCategory(
        [FromRoute] Guid id, 
        ISender sender)
    {
        await sender.Send(new DeleteCategoryCommand { Id = id });
        return ApiResponse.SuccessResult().ToOk();
    }
    
    
    public async Task<Ok<ApiResponse<List<CategoryDto>>>> GetAllCategories(ISender sender)
    {
        var result = await sender.Send(new GetAllCategoriesQuery());
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<CategoryDto>>> GetCategoryById([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new GetCategoryByIdQuery { Id = id });
        return result.ToOk();
    }
    
}
