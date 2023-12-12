namespace bot.src;

public interface IBot
{
    /// <param name="terminationDate">When should the bot stop working(UTC Expected for the timezone)</param>
    public Task Run(DateTime? terminationDate = null);
}
