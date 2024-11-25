using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class RepositoryException(string message ) : Exception (message);
[ExcludeFromCodeCoverage]
public class RepositoryDataIsExistException(string key, string value, string message)
    : Exception($"{message} - [key: {key}, value: {value}]");
[ExcludeFromCodeCoverage]
public class ArgumentNullRepositoryException(string argumentName, string message) : RepositoryException($"{message} - [Argument: {argumentName}]");
