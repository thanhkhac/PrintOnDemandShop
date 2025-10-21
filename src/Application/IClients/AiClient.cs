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
}
