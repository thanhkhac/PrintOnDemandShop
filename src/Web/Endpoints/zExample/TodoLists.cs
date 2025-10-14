﻿using CleanArchitectureBase.Application.zExample.TodoLists.Commands.CreateTodoList;
using CleanArchitectureBase.Application.zExample.TodoLists.Commands.DeleteTodoList;
using CleanArchitectureBase.Application.zExample.TodoLists.Commands.UpdateTodoList;
using CleanArchitectureBase.Application.zExample.TodoLists.Queries.GetTodos;

namespace CleanArchitectureBase.Web.Endpoints.zExample;

public class TodoLists : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // app.MapGroup(this)
        //     // .RequireAuthorization()
        //     .MapGet(GetTodoLists)
        //     .MapPost(CreateTodoList)
        //     .MapPut(UpdateTodoList, "{id}")
        //     .MapDelete(DeleteTodoList, "{id}");
    }

    public Task<TodosVm> GetTodoLists(ISender sender)
    {
        return  sender.Send(new GetTodosQuery());
    }

    public Task<int> CreateTodoList(ISender sender, CreateTodoListCommand command)
    {
        return sender.Send(command);
    }

    public async Task<IResult> UpdateTodoList(ISender sender, int id, UpdateTodoListCommand command)
    {
        if (id != command.Id) return Results.BadRequest();
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> DeleteTodoList(ISender sender, int id)
    {
        await sender.Send(new DeleteTodoListCommand(id));
        return Results.Ok("hello");
    }
}
