using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface
    IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<User> DomainUsers { get; }
    public DbSet<Transaction> Transactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
