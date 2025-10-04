namespace CleanArchitectureBase.Application.Products.Dtos.RequestDtos;

public class UpsertProductOptionRequest
{
    public Guid? OptionId { get; set; }
    public string? Name { get; set; }
    public List<UpsertOptionValueRequest> Values { get; set; } = new();
}

public class UpsertOptionValueRequest
{
    public Guid? OptionValueId { get; set; }
    public string? Value { get; set; }
    public List<string> ImageUrl { get; set; } = new();
}
