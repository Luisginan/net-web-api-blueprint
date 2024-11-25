using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class DaoException(string message) : Exception(message);
[ExcludeFromCodeCoverage]
public class ConnectionDaoException( string message) : DaoException(message);
[ExcludeFromCodeCoverage]
public class QueryNotFoundDaoException(string? queryVariable, string message) : DaoException($"{message} [Query: {queryVariable}]");
[ExcludeFromCodeCoverage]
public class PropertyGeneratedIsNullDaoException(string? property, string message) : DaoException($"{message} [Property: {property}]");
