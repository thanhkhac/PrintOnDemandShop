namespace CleanArchitectureBase.Domain.Entities;

public class SampleImage : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
}
