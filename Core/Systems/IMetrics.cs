using Prometheus;

namespace Core.Systems;
public interface IMetrics
{
    public void InitCounter(string name, string help);
    public void InitHistogram(string name, string help, double[] buckets);
    public void InitGauge(string name, string help);

    public void InitSummary(string name, string help, IReadOnlyList<QuantileEpsilonPair> quantiles);
    public void SetCounter(string name);
    public void SetHistogram(string name, double value);
    public void SetGauge(string name, string label);
    public void SetSummary(string name, double value);
    public bool IsMetricCounterExist(string name);
    public bool IsMetricHistogramExist(string name);
    public bool IsMetricGaugeExist(string name);
}