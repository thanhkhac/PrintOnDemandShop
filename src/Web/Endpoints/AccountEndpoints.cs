using CleanArchitectureBase.Application.Accounts;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class AccountEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        group.MapGet(GetProfile, "Profile");
        group.MapPost(UpdateProfile, "UpdateProfile");
    }


    public async Task<Ok<ApiResponse<UserProfileDto>>> GetProfile(ISender sender, HttpContext httpContext)
    {
        var result = await sender.Send(new GetProfileQuery());
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> UpdateProfile(ISender sender, UpdateProfileCommand request)
    {
        var result = await sender.Send(request);
        return result.ToOk();
    }
}
