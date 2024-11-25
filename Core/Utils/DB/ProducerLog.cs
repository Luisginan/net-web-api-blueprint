using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class ProducerLog : IProducerLog
{

    private readonly IMongoCollection<Message<object>> _kafkaConsumer;
    private readonly ILogger<ProducerLog> _logger;

    public ProducerLog(IOptions<LogDbConfig> logDbConfig, ILogger<ProducerLog> logger, IVault vault)
    {
        _logger = logger;
        var setting = vault.RevealSecret(logDbConfig.Value);
        var connectionString = $"mongodb://{setting.User}:{setting.Password}@{setting.Server}:{setting.Port}";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("log");
        _kafkaConsumer = database.GetCollection<Message<object>>("messaging_log");
    }
        
        
    public async Task DeleteAsync(string key)
    {
        await _kafkaConsumer.DeleteOneAsync(message => message.Key == key);
        _logger.LogInformation("ProducerLog DeleteAsync: {info}","Key= " + key);
    }


    public async Task<Message<object>?> GetAsync(string key)
    {
        var filter = Builders<Message<object>>.Filter.Eq("Key", key);
        var result = await _kafkaConsumer.FindAsync(filter);
        _logger.LogInformation("ProducerLog GetAsync: {info}", "Key= " + key);
        return await result.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(Message<object> message)
    {
        message.Method = "producer";
        await _kafkaConsumer.InsertOneAsync(message);
        _logger.LogInformation("ProducerLog InsertAsync: {info}", "Key= " + message.Key);
            
    }

    public async Task UpdateAsync(string key, Message<object> message)
    {
        message.Method = "producer";
        await _kafkaConsumer.ReplaceOneAsync(c => c.Key == key, message);
        _logger.LogInformation("ProducerLog UpdateAsync: {info}", "Key= " + message.Key);
    }
}