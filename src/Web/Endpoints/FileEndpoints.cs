using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Images;
using CleanArchitectureBase.Application.Payments.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class FileEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(UploadImage, "UploadImage").DisableAntiforgery();
    }

    public async Task<Ok<ApiResponse<string>>> UploadImage(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var fileData = new FileStreamData
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Data = file.OpenReadStream()
        };
        var rq = new UploadImageCommand
        {
            File = fileData
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
