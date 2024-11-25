using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using NawaDataDAL.Common;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public sealed class Connection : IConnection, IDisposable
{
    private NawaConnection? _nawaConnection;
    private bool _disposed;
    private static bool _isConfigure;
    private static readonly object LockObj = new();

    public Connection(IVault vault, IOptions<DatabaseConfig> option)
    {
        lock (LockObj)
        {
            if (!_isConfigure)
            {
                var databaseConfig = vault.RevealSecret(option.Value);
    
                NawaDAO.Config(databaseConfig.Type,
                    databaseConfig.Server,
                    databaseConfig.Database,
                    databaseConfig.User,
                    databaseConfig.Password,
                    databaseConfig.Port,
                    databaseConfig.CommandTimeout,
                    databaseConfig.PoolSize,
                    databaseConfig.ConnectTimeout,
                    "false",
                    "",
                    ""
                );
                _isConfigure = true;
            } 
        }
       
        
        _nawaConnection = NawaDAO.CreateConnection();
    }

    public void BeginTransaction()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Connection));
        _nawaConnection?.BeginTransaction();
    }

    public void Commit()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Connection));
        _nawaConnection?.Commit();
    }

    public void Rollback()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Connection));
        _nawaConnection?.Rollback();
    }

    public object? GetConnection()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Connection));
        return _nawaConnection;
    }

    public bool IsConnectionExist()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Connection));
        return _nawaConnection != null;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) _nawaConnection?.Dispose();

        _nawaConnection = null;
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Connection()
    {
        Dispose(false);
    }
}