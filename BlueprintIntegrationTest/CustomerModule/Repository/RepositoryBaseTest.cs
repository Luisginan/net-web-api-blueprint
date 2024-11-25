using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;

namespace BlueprintIntegrationTest.CustomerModule.Repository;

public abstract class RepositoryBaseTest
{
    protected readonly NawaDaoRepository Repository;
    protected readonly QueryBuilderRepository QueryBuilder;
    [CanBeNull] private static Query _query;

    protected RepositoryBaseTest()
    {
        var vault = new Mock<IVault>();
        vault.Setup(x => x.RevealSecret(It.IsAny<DatabaseConfig>())).Returns(new DatabaseConfig
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
        var options = new Mock<IOptions<DatabaseConfig>>();
        IConnection connection = new Connection(vault:vault.Object, option:options.Object);
        Repository = new NawaDaoRepository(connection);
        QueryBuilder = new QueryBuilderRepository();
        
        if (_query == null)
        {
            _query = new Query();
            _query.SetQueryLocation("./Queries/blueprint.json");
        }
        
    }
}