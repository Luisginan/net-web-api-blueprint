using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class ObjectSetupException(string message) : Exception(message);
[ExcludeFromCodeCoverage]
public class ConfigNotImplementedException(string key, string context) : ObjectSetupException($"Config key: {key} not implemented in {context}");