using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public class LogDbMock(ILogger<LogDbMock> logger) : ILogDb
{
    public int ExecuteNonQuery(string cmdText, IEnumerable<FieldParameter> fieldParameter)
    {
        logger.LogWarning("LogDbMock ExecuteNonQuery");
        return 1;
    }

    public T? ExecuteRow<T>(string cmdText, IEnumerable<FieldParameter> fieldParameter) where T : class, new()
    {
        logger.LogWarning("LogDbMock ExecuteRow");
        return new T();
    }

    public List<T> ExecuteTable<T>(string cmdText, IEnumerable<FieldParameter> fieldParameter) where T : class, new()
    {
        logger.LogWarning("LogDbMock ExecuteTable");
        return [];
    }
}