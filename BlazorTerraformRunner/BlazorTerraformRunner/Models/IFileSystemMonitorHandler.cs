namespace BlazorTerraformRunner.Models
{
    public interface IFileSystemMonitorHandler
    {
        //Task Triggered(WorkspaceConfig workspaceConfig);
        Task Message(WorkspaceConfig workspaceConfig, MessageType messageType, string message);
    }
}
