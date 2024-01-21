using InMemoryCandleRepository = bot.src.Data.InMemory.CandleRepository;
using NoneCandleRepository = bot.src.Data.None.CandleRepository;


namespace bot.src.Data;

public static class CandleRepositoryFactory
{
    public static ICandleRepository CreateRepository(string repositoryType) => repositoryType switch
    {
        CandleRepositoryNames.IN_MEMORY => new InMemoryCandleRepository(),
        CandleRepositoryNames.NONE => new NoneCandleRepository(),
        _ => throw new Exception()
    };
}
