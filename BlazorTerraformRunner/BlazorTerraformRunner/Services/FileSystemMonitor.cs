using BlazorTerraformRunner.Models;
using BlazorTerraformRunner.Utility;
using Microsoft.Extensions.Logging;

namespace BlazorTerraformRunner.Services
{
    public class FileSystemMonitor
    {
        FileSystemWatcher? watcher;
        List<IFileSystemMonitorHandler> handlers = new List<IFileSystemMonitorHandler>();
        WorkspaceConfig? jobConfig;

        public FileSystemMonitor(TerraformRunner terraformRunner, ILogger<FileSystemMonitor> logger)
        {
            this.terraformRunner = terraformRunner ?? throw new ArgumentNullException(nameof(terraformRunner));
            this.logger = logger;
        }
        private readonly TerraformRunner terraformRunner;
        private readonly ILogger<FileSystemMonitor> logger;

        public void EnableWatcher(WorkspaceConfig jobConfig)
        {
            this.jobConfig = jobConfig;
            ArgumentNullException.ThrowIfNullOrEmpty(jobConfig.RootFolderLocal);
            watcher = new FileSystemWatcher(jobConfig.RootFolderLocal);
            watcher.Renamed += FileSystemWatcher_EventAsync;
            watcher.Created += FileSystemWatcher_EventAsync;
            watcher.Deleted += FileSystemWatcher_EventAsync;
            watcher.Changed += FileSystemWatcher_EventAsync;
            watcher.EnableRaisingEvents = true;
        }

        public void RemoveMonitor()
        {
            ArgumentNullException.ThrowIfNull(nameof(watcher));
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        public void AddHandler(IFileSystemMonitorHandler handler)
        {
            if (handlers.Contains(handler)) return;
            handlers.Add(handler);
        }

        public void RemoveHandler(IFileSystemMonitorHandler handler)
        {
            handlers.Remove(handler);
        }

        async Task RunTerraform()
        {
            ArgumentNullException.ThrowIfNull(nameof(jobConfig));
            try
            {
                await terraformRunner.RunTerraform("plan", jobConfig!);
            }
            catch (Exception e)
            {
                handlers.ForEach(a => a.Message(jobConfig!, MessageType.Error, e.Message));
            }
        }

        DateTime dateTimeBackOff = DateTime.MinValue;

        private async void FileSystemWatcher_EventAsync(object sender, FileSystemEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(nameof(jobConfig));
            var fi = new FileInfo(e.FullPath);
            if (jobConfig!.IgnoreFiles?.Contains(fi.Name) ?? false)
            {
                logger.LogInformation($"Ignoring file event {e.ChangeType} for {e.FullPath}");
                return;
            }
            var now = DateTime.UtcNow;
            logger.LogInformation("Backoff: " + dateTimeBackOff.AddSeconds(1).ToString());
            logger.LogInformation("now: " + now.ToString());
            if (dateTimeBackOff > now) { 
                logger.LogInformation($"Ignoring file event {e.ChangeType} for {e.FullPath} due to back-off policy");
                return;
            }
            dateTimeBackOff = now.AddSeconds(1);
            logger.LogInformation($"Running terraform plan due to file event {e.ChangeType} for {e.FullPath}");
            await RunTerraform();
            //await Task.WhenAll(handlers.Select(h => h.Triggered(jobConfig)));
        }
    }
}
