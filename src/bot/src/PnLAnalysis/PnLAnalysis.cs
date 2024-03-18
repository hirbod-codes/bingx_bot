using bot.src.Data;
using bot.src.Data.Models;
using bot.src.PnLAnalysis.Exceptions;
using bot.src.PnLAnalysis.Models;

namespace bot.src.PnLAnalysis;

public static class PnLAnalysis
{
    public static async Task<AnalysisSummary> RunAnalysis(IPositionRepository repo, IMessageRepository messageRepository)
    {
        IEnumerable<Position?> closedPositions = await repo.GetClosedPositions();

        AnalysisSummary analysisSummary = new()
        {
            SignalsCount = (await messageRepository.GetMessages()).Count(),
            OpenedPositions = (await repo.GetOpenedPositions()).Where(o => o != null).Count(),
            PendingPositions = (await repo.GetPendingPositions()).Where(o => o != null).Count(),
            CancelledPositions = (await repo.GetCancelledPositions()).Where(o => o != null).Count(),
            ClosedPositions = closedPositions.Where(o => o != null && !o.UnknownCloseState).Count(),
            UnknownStatePositions = closedPositions.Where(o => o != null && o.UnknownCloseState).Count(),
            LongGrossProfit = GetLongGrossProfit(closedPositions),
            ShortGrossProfit = GetShortGrossProfit(closedPositions),
            LongGrossLoss = GetLongGrossLoss(closedPositions),
            ShortGrossLoss = GetShortGrossLoss(closedPositions)
        };

        foreach (Position? position in closedPositions)
        {
            if (position == null)
                continue;

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
