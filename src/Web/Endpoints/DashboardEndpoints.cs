using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Dashboard;
using CleanArchitectureBase.Application.Orders.User.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class StatisticsEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        
        group.MapGet(GetDashboardData, "DashboardData");
        
    }
    
    public async Task<Ok<ApiResponse<DashBoardData>>> GetDashboardData(
        // [FromBody] GetDashboardDataQuery command,
        ISender sender)
    {
        var request = new GetDashboardDataQuery();
        var result = await sender.Send(request);
        return result.ToOk();
    }
}
