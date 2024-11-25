using System.Threading.Tasks;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreIntegrationTest.Utils.DB;

public class NawaDaoTransactionTest
{

    private static DatabaseConfig GetDatabaseConfig()
    {
        return new DatabaseConfig
        {
            Database = "db-oneloan-alpha",
            Password = "oly8qMSy/cYUC9UZw+DOVQ==",
            Port = "5432",
            Provider = "postgres",
            Server = "34.101.163.123",
            Type = "PostgreSQL",
            User = "postgres",
            CommandTimeout = "30",
            ConnectTimeout = "30",
            PoolSize = "100"
        };
    }

    [Fact]
    public void MultiThreadTransactionTest()
    {
        var task1 = Task.Run(() =>
        {
            var vault = new Mock<IVault>();
            var options = new Mock<IOptions<DatabaseConfig>>();
        
            vault.Setup(m => m.RevealSecret(It.IsAny<object>())).Returns(
                GetDatabaseConfig()
            );

            using var connection = new Connection(vault.Object, options.Object);
            var nawaDaoRepository = new NawaDaoRepository(connection);
            connection.BeginTransaction();
            var result = nawaDaoRepository.ExecuteScalar("SELECT 1");
            connection.Commit();
            Assert.Equal(1, result);
        });
        
        var task2 = Task.Run(() =>
        {
            var vault = new Mock<IVault>();
            var options = new Mock<IOptions<DatabaseConfig>>();
        
            vault.Setup(m => m.RevealSecret(It.IsAny<object>())).Returns(
                GetDatabaseConfig()
            );

            using var connection = new Connection(vault.Object, options.Object);
            var nawaDaoRepository = new NawaDaoRepository(connection);
            connection.BeginTransaction();
            var result = nawaDaoRepository.ExecuteScalar("SELECT 1");
            connection.Commit();
            Assert.Equal(1, result);
        });
        
         Task.WaitAll(task1, task2);
    }
}