using Abstractions.src.Brokers;
using Abstractions.src.Data;
using Abstractions.src.Data.Models;
using Abstractions.src.PnLAnalysis.Exceptions;
using Abstractions.src.PnLAnalysis.Models;
using Abstractions.src.RiskManagement;

namespace Abstractions.src.PnLAnalysis;

public static class PnLAnalysis
{
    public static async Task<AnalysisSummary> RunAnalysis(IPositionRepository repo, IMessageRepository messageRepository, Dictionary<string, object> indicators, IRiskManagement riskManagement, IBroker broker)
    {
        decimal accountBalance = broker.GetBalance();

        IEnumerable<Position> closedPositions = (await repo.GetClosedPositions()).Where(o => o != null)!;

        AnalysisSummary analysisSummary = new()
        {
            UnacceptableOrdersCount = riskManagement.GetUnacceptableOrdersCount(),
            SignalsCount = (await messageRepository.GetMessages()).Count(),
            OpenedPositionsCount = (await repo.GetOpenedPositions()).Where(o => o != null).Count(),
            PendingPositionsCount = (await repo.GetPendingPositions()).Where(o => o != null).Count(),
            CancelledPositionsCount = (await repo.GetCancelledPositions()).Where(o => o != null).Count(),
            ClosedPositionsCount = closedPositions.Where(o => o != null && !o.UnknownCloseState).Count(),
            UnknownStatePositions = closedPositions.Where(o => o != null && o.UnknownCloseState).Count(),
            LongGrossProfit = GetLongGrossProfit(closedPositions),
            ShortGrossProfit = GetShortGrossProfit(closedPositions),
            LongGrossLoss = GetLongGrossLoss(closedPositions),
            ShortGrossLoss = GetShortGrossLoss(closedPositions),
            Indicators = indicators
        };

        foreach (Position position in closedPositions)
        {
            if (position.PositionDirection == PositionDirection.SHORT)
                analysisSummary.ShortPositionCount++;

            if (position.PositionDirection == PositionDirection.LONG)
                analysisSummary.LongPositionCount++;

            analysisSummary.NetProfit += (decimal)position.ProfitWithCommission!;

            if (analysisSummary.NetProfit > analysisSummary.HighestNetProfit)
                analysisSummary.HighestNetProfit = analysisSummary.NetProfit;

            analysisSummary.DrawDown = analysisSummary.HighestNetProfit - analysisSummary.NetProfit;

            if (analysisSummary.DrawDown > analysisSummary.HighestDrawDown)
                analysisSummary.HighestDrawDown = analysisSummary.DrawDown;

            if (position.ProfitWithCommission > 0)
            {
                if (position.PositionDirection == PositionDirection.LONG)
                    analysisSummary.LongWins++;
                else
                    analysisSummary.ShortWins++;
                analysisSummary.Wins++;
            }
            else
            {
                if (position.PositionDirection == PositionDirection.LONG)
                    analysisSummary.LongLosses++;
                else
                    analysisSummary.ShortLosses++;
                analysisSummary.Losses++;
            }
        }

        try { analysisSummary.WinLossRatio = (decimal)((decimal)analysisSummary.Wins / (decimal)(analysisSummary.Losses + analysisSummary.Wins)); }
        catch (DivideByZeroException) { }

        analysisSummary.GrossProfit = analysisSummary.LongGrossProfit + analysisSummary.ShortGrossProfit;
        analysisSummary.GrossLoss = analysisSummary.LongGrossLoss + analysisSummary.ShortGrossLoss;

        analysisSummary.NetProfitPercentage = analysisSummary.NetProfit / accountBalance * 100.0m;
        analysisSummary.HighestNetProfitPercentage = analysisSummary.HighestNetProfit / accountBalance * 100.0m;
        analysisSummary.HighestDrawDownPercentage = analysisSummary.HighestDrawDown / accountBalance * 100.0m;

        if (closedPositions.Any())
        {
            analysisSummary.BuyAndHoldProfit = Math.Abs((decimal)(closedPositions.First().OpenedPrice - closedPositions.Last()!.ClosedPrice)!) * accountBalance / closedPositions.First().OpenedPrice;
            analysisSummary.BuyAndHoldProfitPercentage = analysisSummary.BuyAndHoldProfit / accountBalance * 100.0m;
        }

        return analysisSummary;
    }

    public static decimal GetNetProfit(IEnumerable<Position?> closedPositions) => GetLongNetProfit(closedPositions) + GetShortNetProfit(closedPositions);

    public static decimal GetLongNetProfit(IEnumerable<Position?> closedPositions) => GetLongGrossProfit(closedPositions) - GetLongGrossLoss(closedPositions);

    public static decimal GetShortNetProfit(IEnumerable<Position?> closedPositions) => GetShortGrossProfit(closedPositions) - GetShortGrossLoss(closedPositions);

    public static decimal GetGrossProfit(IEnumerable<Position?> closedPositions) => GetLongGrossProfit(closedPositions) + GetShortGrossProfit(closedPositions);

    public static decimal GetGrossLoss(IEnumerable<Position?> closedPositions) => GetLongGrossLoss(closedPositions) + GetShortGrossLoss(closedPositions);

    public static decimal GetLongGrossProfit(IEnumerable<Position?> closedPositions)
    {
        decimal grossProfit = 0;
        foreach (Position? position in closedPositions)
        {
            if (position == null || position.PositionDirection != PositionDirection.LONG || position.ProfitWithCommission <= 0)
                continue;

            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            grossProfit += (decimal)position.ProfitWithCommission!;
        }

        return grossProfit;
    }

    public static decimal GetShortGrossProfit(IEnumerable<Position?> closedPositions)
    {
        decimal grossProfit = 0;
        foreach (Position? position in closedPositions)
        {
            if (position == null || position.PositionDirection != PositionDirection.SHORT || position.ProfitWithCommission <= 0)
                continue;

            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            grossProfit += (decimal)position.ProfitWithCommission!;
        }

        return grossProfit;
    }

    public static decimal GetLongGrossLoss(IEnumerable<Position?> closedPositions)
    {
        decimal grossLoss = 0;
        foreach (Position? position in closedPositions)
        {
            if (position == null || position.PositionDirection != PositionDirection.LONG || position.ProfitWithCommission >= 0)
                continue;

            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            if (position.PositionDirection == PositionDirection.LONG && position.ProfitWithCommission < 0)
                grossLoss += Math.Abs((decimal)position.ProfitWithCommission!);
        }

        return grossLoss;
    }

    public static decimal GetShortGrossLoss(IEnumerable<Position?> closedPositions)
    {
        decimal grossLoss = 0;
        foreach (Position? position in closedPositions)
        {
            if (position == null || position.PositionDirection != PositionDirection.SHORT || position.ProfitWithCommission >= 0)
                continue;

            if (position.PositionStatus != PositionStatus.CLOSED)
                throw new InvalidPositionException();

            grossLoss += Math.Abs((decimal)position.ProfitWithCommission!);
        }

        return grossLoss;
    }
}
