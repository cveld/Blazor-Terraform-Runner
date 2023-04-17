namespace BlazorTerraformRunner.Services
{
    public class KubernetesMonitor : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
