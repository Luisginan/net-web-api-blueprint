using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class DbTransactionException(string message) : Exception(message);