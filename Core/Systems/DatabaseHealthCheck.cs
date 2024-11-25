using System.Diagnostics.CodeAnalysis;
using Core.Utils.DB;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Systems;
[ExcludeFromCodeCoverage]
public class DatabaseHealthCheck(INawaDaoRepository nawaDaoRepository) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,  CancellationToken cancellationToken = new())
    {
        try
        {
            nawaDaoRepository.ExecuteScalar("SELECT CURRENT_DATE", new List<FieldParameter>());
            return Task.FromResult(HealthCheckResult.Healthy("Database is available"));
        }
        catch (Exception e)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Database is not available", e));
        }
    }
}