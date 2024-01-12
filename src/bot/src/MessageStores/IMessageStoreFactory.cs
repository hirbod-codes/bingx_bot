namespace bot.src.MessageStores;

public interface IMessageStoreFactory
{
    public IMessageStore CreateMessageStore();
}
