using Refit;

namespace CleanArchitectureBase.Application.IClients;

public interface IAiClient
{
    public class AskUserResponse
    {
        public string? Message { get; set; }
    }

    [Post("/ask_user")]
    Task<AskUserResponse> AskUser();
    
    [Post("/update_product")]
    Task CreateProduct(object data);
    
    [Post("/delete_product")]
    Task DeleteProductVariant(object data);
}

public class UpsertProduct
{
    
}
