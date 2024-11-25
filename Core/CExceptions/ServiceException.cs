using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class ServiceException(string message) : Exception(message);
[ExcludeFromCodeCoverage]
public class DataNotFoundServiceException(string key, string value, string message) : ServiceException($"{message} [key: {key}, value: {value}]");
[ExcludeFromCodeCoverage]
public class DataIsExistServiceException : ServiceException
{
    public DataIsExistServiceException(string key, string value, string message) : base($"{message} [key: {key}, value: {value}]")
    {
    }
    
    public DataIsExistServiceException(string message) : base(message)
    {
    }
}
