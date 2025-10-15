using Refit;

namespace CleanArchitectureBase.Infrastructure.HttpClients;

public interface IAiClient
{
    [Post("/ask_user")]
    Task<AskUserResponse> AskUser();
}

public class AskUserResponse
{
    public string? Message { get; set; }
}
