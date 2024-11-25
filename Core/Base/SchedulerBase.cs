using System.Diagnostics.CodeAnalysis;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public abstract class SchedulerBase<T>(ILogger<ProducerBase> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
            await Task.Yield();

            OnPrepare();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var listTask = new List<Task>();
                    var items = GetData();
                    foreach (var t in items.Select(item => new Task(()=> OnExecuteRequest(item))))
                    {
                        listTask.Add(t);
                        t.Start();
                    }
                    
                    Task.WaitAll(listTask.ToArray(), stoppingToken);
                    logger.LogInformation("SchedulerBase ExecuteAsync: {info}", "Scheduler executed");
                    await Task.Delay(Delay(), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
    }

    protected abstract void OnPrepare();
    protected abstract List<T> GetData();

    protected abstract void OnExecuteRequest(T item);

    protected abstract int Delay();
        
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        base.StopAsync(cancellationToken);
        logger.LogInformation("SchedulerBase StopAsync: {info}", "Scheduler stopped");
        return Task.CompletedTask;
    }
}