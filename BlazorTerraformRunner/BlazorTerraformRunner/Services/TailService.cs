using BlazorTerraformRunner.Models;
using k8s;
using k8s.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlazorTerraformRunner.Services
{
    public class TailService: IDisposable
    {
        private readonly KubernetesClient kubernetesClient;
        private readonly MessageService messageService;
        private readonly TailRepository tailRepository;

        public TailService(KubernetesClient kubernetesClient, MessageService messageService, TailRepository tailRepository)
        {
            this.kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            this.messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            this.tailRepository = tailRepository ?? throw new ArgumentNullException(nameof(tailRepository));
            tailRepository.PropertyChanged += TailRepository_PropertyChanged;
            MonitorPod();
        }

        private void TailRepository_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MonitorPod();
        }

        

        Watcher<V1Pod>? watcher;
        string? currentPodName;

        public void MonitorPod()
        {
            var podName = tailRepository.CurrentJobRun?.PodName;

            if (currentPodName == podName)
            {
                return;
            }
            currentPodName = podName;
            if (podName == null)
            {
                messageService.ShowError($"MonitorPod: {nameof(podName)} null");
            }
            var podListResponseTask = kubernetesClient.client.CoreV1.ListNamespacedPodWithHttpMessagesAsync("default", watch: true, fieldSelector: $"metadata.name={podName}");
            watcher?.Dispose();
            watcher = podListResponseTask.Watch<V1Pod, V1PodList>(
                onError: (err) =>
                {
                    messageService.ShowError(err.Message);
                    //errors += $"Exceptions: {err.Message}<br>\n";
                },
                onEvent: (t, i) => Handler(t, i));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        void Handler(WatchEventType type, V1Pod item)
        {
            if (tailRepository.CurrentJobRun?.PodName == item.Name())
            {
                tailRepository.SetPodStatus(item.Status.Phase);
            }
        }

        public void Dispose()
        {
            tailRepository.PropertyChanged -= TailRepository_PropertyChanged;
            watcher?.Dispose();
        }
    }
}
