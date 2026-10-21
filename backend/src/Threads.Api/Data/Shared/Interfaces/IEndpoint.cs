namespace Threads.Api.Data.Shared.Interfaces;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder builder);
}
