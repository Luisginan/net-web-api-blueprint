using System.Diagnostics.CodeAnalysis;

namespace Core.CExceptions;
[ExcludeFromCodeCoverage]
public class MetricsException(string message) : Exception (message);
[ExcludeFromCodeCoverage]
public class ItemNotFoundMetricsException(string metricName) : MetricsException ($"Item not found: {metricName} in list of metrics");