using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class TopicManagerException(string message) : Exception(message);