namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IHangFireService
{
    Task DeleteJobByArgument(string content);
}
