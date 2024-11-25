namespace Core.Utils.DB;

public interface IConnection
{
    void BeginTransaction();
    void Commit();
    void Rollback();
    object? GetConnection();
    
}