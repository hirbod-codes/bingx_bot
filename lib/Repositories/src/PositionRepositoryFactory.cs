using Abstractions.src.Data;
using InMemoryPositionRepository = Data.src.InMemory.PositionRepository;
using NonePositionRepository = Data.src.None.PositionRepository;

namespace Data.src;

public static class PositionRepositoryFactory
{
    public static IPositionRepository CreateRepository(string repositoryType) => repositoryType switch
    {
        PositionRepositoryNames.IN_MEMORY => new InMemoryPositionRepository(),
        PositionRepositoryNames.NONE => new NonePositionRepository(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        PositionRepositoryNames.IN_MEMORY => typeof(InMemoryPositionRepository),
        PositionRepositoryNames.NONE => typeof(NonePositionRepository),
        _ => throw new Exception()
    };
}
