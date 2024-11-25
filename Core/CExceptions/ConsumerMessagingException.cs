using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class ConsumerMessagingException(string message) : Exception(message);
[ExcludeFromCodeCoverage]
public class DeserializeConsumerMessagingException(string message) : ConsumerMessagingException("Failed convert to Models. message:" + message);