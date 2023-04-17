using BlazorTerraformRunner.Models;
using BlazorTerraformRunner.Pages;
using System.Collections;
using System.Collections.Concurrent;

namespace BlazorTerraformRunner.Services
{
    public class MessageRepository
    {
        public ConcurrentDictionary<Guid, Message> Messages { get; set; } = new ConcurrentDictionary<Guid, Message>();
        public MessageRepository()
        {

        }
    }
}
