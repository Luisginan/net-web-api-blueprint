using System.Diagnostics.CodeAnalysis;
using Prometheus;

namespace Core.Systems;

[ExcludeFromCodeCoverage]
public class MetricsMock(ILogger<MetricsMock> logger) : IMetrics
{
    public void InitCounter(string name, string help)
    {
        logger.LogWarning("MetricsMock InitCounter: {info}", $"name={name}, help={help} using Mock");
    }

    public void InitHistogram(string name, string help, double[] buckets)
    {
        logger.LogWarning("MetricsMock InitHistogram: {info}", $"name={name}, help={help}, buckets={buckets} using Mock");
    }

    public void InitGauge(string name, string help)
    {
        logger.LogWarning("MetricsMock InitGauge: {info}", $"name={name}, help={help} using Mock");
    }

    public void InitSummary(string name, string help, IReadOnlyList<QuantileEpsilonPair> quantiles)
    {
        logger.LogWarning("MetricsMock InitSummary: {info}", $"name={name}, help={help} using Mock");
    }

    public void SetCounter(string name)
    {
        logger.LogWarning("MetricsMock SetCounter: {info}", $"name={name}, using Mock");
    }

    public void SetHistogram(string name, double value)
    {
        logger.LogWarning("MetricsMock SetHistogram: {info}", $"name={name}, value={value} using Mock");
    }

    public void SetGauge(string name, string help)
    {
       logger.LogWarning("MetricsMock SetGauge: {info}", $"name={name}, help={help} using Mock");
    }

    public void SetSummary(string name, double value)
    {
        logger.LogWarning("MetricsMock SetSummary: {info}", $"name={name}, value={value} using Mock");
    }

    public bool IsMetricCounterExist(string name)
    {
        return true;
    }

    public bool IsMetricHistogramExist(string name)
    {
        return true;
    }

    public bool IsMetricGaugeExist(string name)
    {
        return true;
    }
}