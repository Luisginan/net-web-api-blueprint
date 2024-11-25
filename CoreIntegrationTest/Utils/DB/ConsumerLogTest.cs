using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.CExceptions;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CoreIntegrationTest.Utils.DB;

public class ConsumerLogTest
{
    [Fact]
    public async Task GetListTest()
    {
        
        // Arrange
         var optionsMock = new Mock<IOptions<DatabaseConfig>>();
 
         var loggerMock = new Mock<ILogger<ConsumerLogPostgres>>();
         var vaultMock = new Mock<IVault>();
         
         vaultMock.Setup(x => x.RevealSecret(It.IsAny<DatabaseConfig>())).Returns(new DatabaseConfig
         {
             Database = "blueprint",
             Password = "VCt/m8/zEfD5MN61wPTfrQ==",
             Port = "5432",
             Provider = "postgres",
             Server = "localhost",
             Type = "PostgreSQL",
             User = "postgres",
             CommandTimeout = "30",
             ConnectTimeout = "30",
             PoolSize = "100"
         });
         
         var logDb = new LogPostgres(vaultMock.Object, optionsMock.Object);

         var key = Guid.NewGuid().ToString();
         var topic = "test-topic";
         var payLoad = JsonConvert.SerializeObject(new Customer
         {
             Name = "test"
         });
         
         logDb.ExecuteNonQuery("DELETE FROM messaging_log WHERE topic = @topic", new List<FieldParameter>
         {
             new("@topic", topic)
         });
         
         logDb.ExecuteNonQuery("INSERT INTO messaging_log (key, app_id, method, retry, topic, group_id, status, error, payload, updated_at, created_at) VALUES (@key, @app_id, @method, @retry, @topic, @group_id, @status, @error, @payload,  @updated_at, @created_at)", new List<FieldParameter>
         {
             new("@key", key),
             new("@app_id", "app_id"),
             new("@method", "method"),
             new("@retry", 0),
             new("@topic", topic),
             new("@group_id", "group_id"),
             new("@status", "DEAD"), 
             new("@error", new ServiceException("test").ToString()),
             new("@payload", payLoad),
             new("@updated_at", DateTime.Now),
             new("@created_at", DateTime.Now)
         });
         var consumerLog = new ConsumerLogPostgres(logDb, loggerMock.Object);
        
        // Act
        var result  = await consumerLog.GetListAsync([topic]);
        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(topic, result[0].Topic);
        Assert.Equal("group_id", result[0].GroupId);
        Assert.Equal("app_id", result[0].AppId);
        Assert.Equal(key, result[0].Key);
        Assert.Equal(0, result[0].Retry);
        Assert.Equal("DEAD", result[0].Status);
        Assert.Equal("method", result[0].Method);
        Assert.NotNull(result[0].Error);
        var payloadDb = result[0].PayLoad.ToString();
        Assert.Equal(payLoad,payloadDb );
        
        var customer = JsonConvert.DeserializeObject<Customer>(payloadDb ?? throw new InvalidOperationException());
        Assert.NotNull(customer);
        Assert.Equal("test", customer.Name);
        
        await consumerLog.UpdateAsync(key, new Message<object>
        {
            Topic = topic,
            GroupId = "group_id",
            AppId = "app_id",
            Key = key,
            PayLoad = payloadDb,
            Retry = 0,
            Error = "",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = "DEAD"
        });
        
        var result2  = await consumerLog.GetAsync(key);
        Assert.NotNull(result2);
        Assert.Equal(payLoad, result2.PayLoad?.ToString());

        await consumerLog.DeleteAsync(key);
        
        var result3  = await consumerLog.GetAsync(key);
        Assert.Null(result3);
        
    }
}