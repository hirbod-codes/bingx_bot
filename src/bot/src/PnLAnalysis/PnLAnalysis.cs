using bot.src.Data;

namespace bot.src.PnLAnalysis;

public class PnLAnalysis
{
    private readonly IPositionRepository _positionRepository;

    public PnLAnalysis(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task RunAnalysis()
    {
    }
}
