using BlazorTerraformRunner.Models;
using BlazorTerraformRunner.Utility;

namespace BlazorTerraformRunner.Services
{

    public class FileSystemMonitors
    {
        public FileSystemMonitors(TerraformRunner terraformRunner, Func<FileSystemMonitor> fileSystemMonitorFactory)
        {
            this.terraformRunner = terraformRunner;
            this.fileSystemMonitorFactory = fileSystemMonitorFactory;
        }
        Dictionary<string, FileSystemMonitor> watchers = new Dictionary<string, FileSystemMonitor>();
        private readonly TerraformRunner terraformRunner;
        private readonly Func<FileSystemMonitor> fileSystemMonitorFactory;

        public FileSystemMonitor AddMonitor(WorkspaceConfig jobConfig)
        {
            //if (watchers.TryGetValue(jobConfig.JobName, out var existingFileSystemMonitor))
            //{
            //    return existingFileSystemMonitor;
            //}
            var fileSystemMonitor = fileSystemMonitorFactory();
            fileSystemMonitor.EnableWatcher(jobConfig);
            watchers.Add(jobConfig.JobName, fileSystemMonitor);
            return fileSystemMonitor;
        }

        public FileSystemMonitor? GetMonitor(WorkspaceConfig jobConfig)
        {
            watchers.TryGetValue(jobConfig.JobName, out var existingFileSystemMonitor);
            return existingFileSystemMonitor;
        }

            public void RemoveMonitor(WorkspaceConfig jobConfig)
        {
            watchers[jobConfig.JobName].RemoveMonitor();
            watchers.Remove(jobConfig.JobName);
        }
    }
}
