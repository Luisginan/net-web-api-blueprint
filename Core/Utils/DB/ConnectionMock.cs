using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public class ConnectionMock : IConnection
{
    private object? _connection;
    public void BeginTransaction()
    {
        
    }

    public void Commit()
    {
        
    }

    public void Rollback()
    {
        
    }

    public void Dispose()
    {
        
    }

    public object? GetConnection()
    {
        return _connection;
    }

    public void SetConnection(object connection)
    {
        _connection = connection;
    }
}