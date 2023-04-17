using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyREST
{
    public class MyRestHealth : IHealthCheck
    {
        private AppState _appState;

        public MyRestHealth(AppState appState)
        {
            _appState = appState;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("aaaaaaaaa"));

            //new HealthCheckResult()
            //context.Registration.FailureStatus
            //throw new NotImplementedException();
        }
    }
}