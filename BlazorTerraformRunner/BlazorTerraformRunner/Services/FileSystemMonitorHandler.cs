using BlazorTerraformRunner.Models;

namespace BlazorTerraformRunner.Services
{
    public class FileSystemMonitorHandler: IFileSystemMonitorHandler
    {
        private readonly MessageService messageService;

        public FileSystemMonitorHandler(MessageService messageService)
        {
            this.messageService = messageService;
        }

        public Task Message(WorkspaceConfig workspaceConfig, MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.Information:
                    messageService.ShowInfo(message);
                    break;
                case MessageType.Error:
                    messageService.ShowError(message);
                    break;
                default:
                    messageService.ShowError($"Unsupported messagetype: {messageType} with message {message}");
                    break;
            }

            return Task.CompletedTask;
        }

    }
}
