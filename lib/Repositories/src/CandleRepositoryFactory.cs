using Abstractions.src.Repository;
using InMemoryCandleRepository = Repositories.src.InMemory.CandleRepository;

namespace Repositories.src;

public static class CandleRepositoryFactory
{
    public static ICandleRepository CreateRepository(string repositoryType) => repositoryType switch
    {
        CandleRepositoryNames.IN_MEMORY => new InMemoryCandleRepository(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        CandleRepositoryNames.IN_MEMORY => typeof(InMemoryCandleRepository),
        _ => throw new Exception()
    };
}
