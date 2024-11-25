namespace Core.Utils.DB;

public interface IConsumerLog
{
    Task InsertAsync(Message<object> message);
    Task UpdateAsync(string key, Message<object> message);
    Task<Message<object>?> GetAsync(string key);
    Task<List<Message<object>>> GetListAsync(List<string> listTopic);
    Task DeleteAsync(string key);
}