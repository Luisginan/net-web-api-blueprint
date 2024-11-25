using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class ProducerLogMock(ILogger<ProducerLogMock> logger) : IProducerLog
{
    public Task InsertAsync(Message<object> message)
    {
        logger.LogWarning("ProducerLogMock InsertAsync: using mock producer log");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(string key, Message<object> message)
    {
        logger.LogWarning("ProducerLogMock UpdateAsync: using mock producer log");
        return Task.CompletedTask;
    }

    public Task<Message<object>?> GetAsync(string key)
    {
        logger.LogWarning("ProducerLogMock GetAsync: using mock producer log");
        return Task.FromResult<Message<object>?>(null);
    }

    public Task DeleteAsync(string key)
    {
        logger.LogWarning("ProducerLogMock DeleteAsync: using mock producer log");
        return Task.CompletedTask;
    }
}