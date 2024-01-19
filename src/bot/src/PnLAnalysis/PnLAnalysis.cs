using bot.src.Data;
using bot.src.Data.Models;
using bot.src.PnLAnalysis.Models;

namespace bot.src.PnLAnalysis;

public class PnLAnalysis
{
    private readonly IPositionRepository _positionRepository;

    public PnLAnalysis(IPositionRepository positionRepository) => _positionRepository = positionRepository;

    public async Task<StrategySummary> RunAnalysis()
    {
        IEnumerable<Position> closedPositions = await _positionRepository.GetClosedPositions();

        StrategySummary strategySummary = new();

        foreach (Position position in closedPositions)
        {
            strategySummary.NetProfit += (decimal)position.ProfitWithCommission!;

            if (strategySummary.NetProfit > strategySummary.HighestNetProfit)
                strategySummary.HighestNetProfit = strategySummary.NetProfit;

            strategySummary.DraDown += strategySummary.HighestNetProfit - strategySummary.NetProfit;

            if (strategySummary.DraDown > strategySummary.HighestDrawDown)
                strategySummary.HighestDrawDown = strategySummary.DraDown;

            if (position.PositionDirection == PositionDirection.LONG)
                if (position.ProfitWithCommission > 0)
                    strategySummary.LongGrossProfit += (decimal)position.ProfitWithCommission;
                else
                    strategySummary.LongGrossLoss += (decimal)position.ProfitWithCommission;
            else
                if (position.ProfitWithCommission > 0)
                strategySummary.ShortGrossProfit += (decimal)position.ProfitWithCommission;
            else
                strategySummary.ShortGrossLoss += (decimal)position.ProfitWithCommission;
        }

        strategySummary.GrossProfit = strategySummary.LongGrossProfit + strategySummary.ShortGrossProfit;
        strategySummary.GrossLoss = strategySummary.LongGrossLoss + strategySummary.ShortGrossLoss;

        return strategySummary;
    }
}
