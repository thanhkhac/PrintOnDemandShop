using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tokens.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureBase.Web.Endpoints;

public class AiTokenEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(DecreaseUserToken, "/DecreaseUserToken");
    }

    /// <summary>
    /// Giảm token của người dùng hiện tại
    /// </summary>
    public async Task<Ok<ApiResponse<int>>> DecreaseUserToken(
        ISender sender)
    {
        var command = new DecreaseUserTokenCommand();
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
