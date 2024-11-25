using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class ConsumerLogMock(ILogger<ConsumerLogMock> logger) : IConsumerLog
{
    public async Task<List<Message<object>>> GetListAsync(List<string> listTopic)
    {
        logger.LogWarning("ConsumerLogMock GetListAsync: using mock consumer log");
        return new List<Message<object>>();
    }

    public Task DeleteAsync(string key)
    {
        logger.LogWarning("ConsumerLogMock DeleteAsync: using mock consumer log");
        return Task.CompletedTask;
    }

    public Task<Message<object>?> GetAsync(string key)
    {
        logger.LogWarning("ConsumerLogMock GetAsync: using mock consumer log");
        return Task.FromResult<Message<object>?>(null);
    }

    public Task InsertAsync(Message<object> message)
    {
        logger.LogWarning("ConsumerLogMock InsertAsync: using mock consumer log");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(string key, Message<object> message)
    {
        logger.LogWarning("ConsumerLogMock UpdateAsync: using mock consumer log");
        return Task.CompletedTask;
    }

}