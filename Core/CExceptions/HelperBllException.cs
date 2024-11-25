using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class HelperBllException(string message) : Exception(message);
[ExcludeFromCodeCoverage]
public class QueryNotFoundHelperBllException(string queryString) : HelperBllException($"Query not found: {queryString}");
[ExcludeFromCodeCoverage]
public class FailedToExecuteQueryHelperBllException(string message) : HelperBllException(message);