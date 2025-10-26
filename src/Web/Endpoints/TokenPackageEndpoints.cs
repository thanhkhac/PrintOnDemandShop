using CleanArchitectureBase.Application.TokenPackages.Commands;
using CleanArchitectureBase.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class TokenPackageEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("/Buy", Buy);
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
}
