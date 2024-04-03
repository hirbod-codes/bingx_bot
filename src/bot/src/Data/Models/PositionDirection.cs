
namespace bot.src.Data.Models;

public class PositionDirection
{
    public const string LONG = "long";
    public const string SHORT = "short";

    public static string Parse(string positionSide) => positionSide.ToLower();
}
