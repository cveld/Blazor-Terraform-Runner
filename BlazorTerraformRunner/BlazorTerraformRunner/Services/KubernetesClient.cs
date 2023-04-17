using k8s;

namespace BlazorTerraformRunner.Services
{
    public class KubernetesClient
    {
        public Kubernetes client;

        public KubernetesClient() {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            client = new Kubernetes(config);
        }
    }
}
