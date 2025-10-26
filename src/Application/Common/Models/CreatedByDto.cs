namespace CleanArchitectureBase.Application.Common.Models;

public class CreatedByDto
{
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}
