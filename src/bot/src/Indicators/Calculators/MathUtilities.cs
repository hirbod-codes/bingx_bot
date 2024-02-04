namespace bot.src.Indicators.Calculators;

/// <summary>
/// This class is a copy of Skender.Stock.Indicators.NullMath class
/// </summary>
public static class MathUtilities
{
    public static double? Abs(this double? value)
    {
        if (value.HasValue)
        {
            return (value < 0.0) ? (0.0 - value).Value : value.Value;
        }

        return null;
    }

    public static decimal? Round(this decimal? value, int digits)
    {
        if (value.HasValue)
        {
            return Math.Round(value.Value, digits);
        }

        return null;
    }

    public static double? Round(this double? value, int digits)
    {
        if (value.HasValue)
        {
            return Math.Round(value.Value, digits);
        }

        return null;
    }

    public static double Round(this double value, int digits)
    {
        return Math.Round(value, digits);
    }

    public static decimal Round(this decimal value, int digits)
    {
        return Math.Round(value, digits);
    }

    public static double Null2NaN(this double? value)
    {
        if (value.HasValue)
        {
            return value.Value;
        }

        return double.NaN;
    }

    public static double? NaN2Null(this double? value)
    {
        if (!value.HasValue || !double.IsNaN(value.GetValueOrDefault()))
        {
            return value;
        }

        return null;
    }

    public static double? NaN2Null(this double value)
    {
        if (!double.IsNaN(value))
        {
            return value;
        }

        return null;
    }
}
