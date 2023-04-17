using Blazored.Toast.Services;
using BlazorTerraformRunner.Models;

namespace BlazorTerraformRunner.Services
{
    public class MessageService
    {
        private readonly IToastService toastService;
        private readonly MessageRepository messageRepository;

        public MessageService(IToastService toastService, MessageRepository messageRepository)
        {
            this.toastService = toastService;
            this.messageRepository = messageRepository;
        }

        public void ShowInfo(string messageText)
        {
            var message = new Message
            {
                MessageType = MessageType.Information,
                Text = messageText
            };
            messageRepository.Messages.TryAdd(Guid.NewGuid(), message);

            toastService.ShowInfo(messageText);
        }

        public void ShowError(string messageText)
        {
            var message = new Message
            {
                MessageType = MessageType.Error,
                Text = messageText
            };
            messageRepository.Messages.TryAdd(Guid.NewGuid(), message);

            toastService.ShowError(messageText);
        }
    }
}
