using bot.src.Data.Models;
using bot.src.PnLAnalysis.Exceptions;
using bot.src.PnLAnalysis.Models;

namespace bot.src.PnLAnalysis;

public static class PnLAnalysis
{
    public static AnalysisSummary RunAnalysis(IEnumerable<Position> closedPositions)
    {
        AnalysisSummary analysisSummary = new();

        foreach (Position position in closedPositions)
        {
            analysisSummary.NetProfit += (decimal)position.ProfitWithCommission!;

            if (analysisSummary.NetProfit > analysisSummary.HighestNetProfit)
                analysisSummary.HighestNetProfit = analysisSummary.NetProfit;

            analysisSummary.DrawDown = analysisSummary.HighestNetProfit - analysisSummary.NetProfit;

            if (analysisSummary.DrawDown > analysisSummary.HighestDrawDown)
                analysisSummary.HighestDrawDown = analysisSummary.DrawDown;

            if (position.PositionDirection == PositionDirection.LONG)
                if (position.ProfitWithCommission > 0)
                    analysisSummary.LongGrossProfit += (decimal)position.ProfitWithCommission;
                else
                    analysisSummary.LongGrossLoss += (decimal)position.ProfitWithCommission;
            else
                if (position.ProfitWithCommission > 0)
                analysisSummary.ShortGrossProfit += (decimal)position.ProfitWithCommission;
            else
                analysisSummary.ShortGrossLoss += (decimal)position.ProfitWithCommission;
        }

        analysisSummary.GrossProfit = analysisSummary.LongGrossProfit + analysisSummary.ShortGrossProfit;
        analysisSummary.GrossLoss = analysisSummary.LongGrossLoss + analysisSummary.ShortGrossLoss;

        return analysisSummary;
    }

    public static decimal GetNetProfit(IEnumerable<Position> closedPositions) => GetLongNetProfit(closedPositions) + GetShortNetProfit(closedPositions);

    public static decimal GetLongNetProfit(IEnumerable<Position> closedPositions) => GetLongGrossProfit(closedPositions) - GetLongGrossLoss(closedPositions);

    public static decimal GetShortNetProfit(IEnumerable<Position> closedPositions) => GetShortGrossProfit(closedPositions) - GetShortGrossLoss(closedPositions);

    public static decimal GetGrossProfit(IEnumerable<Position> closedPositions) => GetLongGrossProfit(closedPositions) + GetShortGrossProfit(closedPositions);

    public static decimal GetGrossLoss(IEnumerable<Position> closedPositions) => GetLongGrossLoss(closedPositions) + GetShortGrossLoss(closedPositions);

    public static decimal GetLongGrossProfit(IEnumerable<Position> closedPositions)
    {
        decimal grossProfit = 0;
        foreach (Position position in closedPositions)
        {
            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            if (position.PositionDirection == PositionDirection.LONG && position.ProfitWithCommission > 0)
                grossProfit += (decimal)position.ProfitWithCommission!;
        }

        return grossProfit;
    }

    public static decimal GetShortGrossProfit(IEnumerable<Position> closedPositions)
    {
        decimal grossProfit = 0;
        foreach (Position position in closedPositions)
        {
            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            if (position.PositionDirection == PositionDirection.SHORT && position.ProfitWithCommission > 0)
                grossProfit += (decimal)position.ProfitWithCommission!;
        }

        return grossProfit;
    }

    public static decimal GetLongGrossLoss(IEnumerable<Position> closedPositions)
    {
        decimal grossLoss = 0;
        foreach (Position position in closedPositions)
        {
            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            if (position.PositionDirection == PositionDirection.LONG && position.ProfitWithCommission < 0)
                grossLoss += Math.Abs((decimal)position.ProfitWithCommission!);
        }

        return grossLoss;
    }

    public static decimal GetShortGrossLoss(IEnumerable<Position> closedPositions)
    {
        decimal grossLoss = 0;
        foreach (Position position in closedPositions)
        {
            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            if (position.PositionDirection == PositionDirection.SHORT && position.ProfitWithCommission < 0)
                grossLoss += Math.Abs((decimal)position.ProfitWithCommission!);
        }

        return grossLoss;
    }
}
