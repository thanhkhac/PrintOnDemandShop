using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureBase.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? PaymentId { get; set; }
    [MaxLength(255)]
    public string? Code { get; set; }
    public string? Gateway { get; set; }
    public string? TransferType { get; set; }
    public decimal TransferAmount { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? SubAccount { get; set; }
    public decimal? Accumulated { get; set; }
    public string? Content { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? Created { get; set; } = DateTimeOffset.UtcNow;
    public User? User { get; set; }

}

public class TokenPackage
{
    public Guid Id { get; set; }
    public int TokenAmount { get; set; }
    public long Price { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserTokenPackage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int TokenAmount { get; set; }
    public long Price { get; set; }
    public string? PaymentCode { get; set; }
    public bool IsPaid { get; set; }
    public DateTimeOffset? TimeEnd { get; set; }
}

