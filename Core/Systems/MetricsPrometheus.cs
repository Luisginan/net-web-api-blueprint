using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Core.CExceptions;
using Prometheus;

namespace Core.Systems;

[ExcludeFromCodeCoverage]
public class MetricsPrometheus : IMetrics
{
    private static readonly List<Counter> ListCounter = [];
    private static readonly List<Histogram> ListHistogram = [];
    private static readonly List<Gauge> ListGauge = [];
    private static readonly List<Summary> ListSummary = [];
    
    public void SetSummary(string name,double value)
    {
        var summary = GetSummary(name);
        summary.Observe(value);
    }
    
    public void InitSummary(string name, string help, IReadOnlyList<QuantileEpsilonPair> quantiles)
    {
        var summaryConfig = new SummaryConfiguration
        {
            SuppressInitialValue = true,
            Objectives = quantiles
        };
        var summary = Metrics.CreateSummary(name, help, summaryConfig);
        ListSummary.Add(summary);
    }

    private static Summary GetSummary(string name)
    {
        var summary =  ListSummary.Find(x => x.Name == name);
        if (summary == null) 
            throw new ItemNotFoundMetricsException(name);
        return summary;
    }
    public void SetHistogram(string name, double value)
    {
        var histogram = GetHistogram(name);
        histogram.Observe(value);
    }

    private static Histogram GetHistogram(string name)
    {
        var histogram =  ListHistogram.Find(x => x.Name == name);
        if (histogram == null) 
            throw new ItemNotFoundMetricsException(name);
        return histogram;
    }

    public void InitCounter(string name, string help)
    {
        var counter = Metrics.CreateCounter(name, help);
        ListCounter.Add(counter);
    }

    public void InitHistogram(string name, string help, double[] buckets)
    {
        var histogramConfig = new HistogramConfiguration
        {
            SuppressInitialValue = true,
            Buckets = buckets
        };
        var histogram = Metrics.CreateHistogram(name, help,histogramConfig);
        ListHistogram.Add(histogram);
    }

    public void InitGauge(string name, string help)
    {
        var gaugeConfig = new GaugeConfiguration
        {
            SuppressInitialValue = true
        };
        
        var gauge = Metrics.CreateGauge(name, help, gaugeConfig);
        ListGauge.Add(gauge);
    }

    public void SetCounter(string name)
    {
        var counter = GetCounter(name);
        counter.Inc();
    }
    
    public void SetGauge(string name, string label)
    {
        var gauge = GetGauge(name);
        gauge.Labels(label).Inc();
    }
    
    private static Gauge GetGauge(string name)
    {
        var gauge =  ListGauge.Find(x => x.Name == name);
        if (gauge == null)
            throw new ItemNotFoundMetricsException(name);

        return gauge;
    }
    
    private static Counter GetCounter(string name)
    {
        var counter =  ListCounter.Find(x => x.Name == name);
        if (counter == null)
            throw new ItemNotFoundMetricsException(name);

        return counter;
    }

    public bool IsMetricCounterExist(string name)
    {
        var counter = ListCounter.Find(x => x.Name == name);
        return counter != null;
    }

    public bool IsMetricHistogramExist(string name)
    {
        var summary = ListSummary.Find(x => x.Name == name);
        return summary != null;
    }

    public bool IsMetricGaugeExist(string name)
    {
        var gauge = ListGauge.Find(x => x.Name == name);
        return gauge != null;
    }
}