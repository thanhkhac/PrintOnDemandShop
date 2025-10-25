using CleanArchitectureBase.Application.Carts;
using CleanArchitectureBase.Application.Carts.Commands;
using CleanArchitectureBase.Application.Carts.Dtos.Response;
using CleanArchitectureBase.Application.Carts.Queries;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.SampleImages;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class SampleImageEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(AddImage, "");
        group.MapDelete(DeleteImage, "");
        group.MapGet(GetAllSampleImages, "");
    }

    public async Task<Ok<ApiResponse<Guid>>> AddImage(
        [FromBody] AddSampleImageCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }
    
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteImage(
        [FromBody] DeleteSampleImageCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }



    public async Task<Ok<ApiResponse<List<string>>>> GetAllSampleImages(
        ISender sender)
    {
        var query = new GetImageQuery();
        var result = await sender.Send(query);
        return result.ToOk();
    }
}
