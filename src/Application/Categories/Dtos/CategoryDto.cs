namespace CleanArchitectureBase.Application.Categories.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }
    
    public List<CategoryDto> SubCategories { get; set; } = new();
    
    public string? Name { get; set; }

}
