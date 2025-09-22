﻿using CleanArchitectureBase.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitectureBase.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitectureBase.Application.zExample.TodoLists.Commands.CreateTodoList;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FunctionalTests.zExample.TodoLists.Commands;

using static Testing;

public class DeleteTodoListTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new DeleteTodoListCommand(99);
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteTodoList()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        await SendAsync(new DeleteTodoListCommand(listId));

        var list = await FindAsync<TodoList>(listId);

        list.Should().BeNull();
    }
}
