using BlazorTerraformRunner.Models;
using BlazorTerraformRunner.Services;
using k8s.Models;
using k8s;

namespace BlazorTerraformRunner.Utility
{
    public class TerraformRunner
    {
        KubernetesClient client;
        private readonly TailRepository tailRepository;

        public TerraformRunner(KubernetesClient client, TailRepository tailRepository)
        {
            this.client = client;
            this.tailRepository = tailRepository;
        }

        public async ValueTask<string> RunTerraform(string command, WorkspaceConfig workspaceConfig)
        {
            var guid = Guid.NewGuid();
            var name = $"terraform-{command}-{guid}";
            var logfile = $"{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}_{guid}.log";
            
            var extracfg = "";
            var extracfg2 = "";
            if (command == "init")
            {
                extracfg = $" -backend-config=\"{workspaceConfig.BackendConfig}\"";
            }
            if (command == "plan")
            {
                extracfg = $" -out=/var/log/terraform.plan";
                extracfg2 = $" && terraform -chdir=\"/var/rootfolder/{workspaceConfig.RootModule}\" show -json /var/log/terraform.plan > /var/log/terraform.plan.json 2>> /var/log/{logfile}";
            }


            tailRepository.CurrentJobRun = new JobRun
            {
                Command = command,
                Id = guid,
                LogFile = logfile,
                PodName = name
            };

            var pod = new V1Pod(
                metadata: new V1ObjectMeta(
                    name: name,
                    labels: new Dictionary<string, string>
                    {
                        { "id", guid.ToString() },
                        { "logfile", logfile }
                    }
                ),
                spec: new V1PodSpec(
                    containers: new List<V1Container>
                    {
                    new V1Container(
                        name: "terraform",
                        image: "alpine-terraform-azure-cli-test",
                        imagePullPolicy: "IfNotPresent",
                        command: new string[]
                        {
                            "sh",
                            "-c",
                            $"""terraform -chdir="/var/rootfolder/{workspaceConfig.RootModule}" {command}{extracfg} > /var/log/{logfile} 2>&1{extracfg2}"""
                        },
                        volumeMounts: new List<V1VolumeMount>
                        {
                            new V1VolumeMount(name: "logs", mountPath: "/var/log"),
                            new V1VolumeMount(name: "azurecli", mountPath: "/var/azurecli"),
                            new V1VolumeMount(name: "rootfolder", mountPath: "/var/rootfolder")
                        },
                        env: new List<V1EnvVar>
                        {
                            new V1EnvVar("AZURE_CONFIG_DIR", "/var/azurecli")
                        }
                    )
                    },
                    restartPolicy: "Never",
                    volumes: new List<V1Volume>
                    {
                    new V1Volume("logs", hostPath: new V1HostPathVolumeSource(
                        path: $"/run/desktop/mnt/host/c/temp/terraform/{guid}", type: "DirectoryOrCreate")),
                    new V1Volume("azurecli", hostPath: new V1HostPathVolumeSource(
                        path: workspaceConfig.AzureCli, type: "Directory")),
                    new V1Volume("rootfolder", hostPath: new V1HostPathVolumeSource(
                        path: workspaceConfig.RootFolderContainer, type: "Directory")
                    )
                    }
                )
            );

            //var pod2 = new V1Pod(
            //    metadata: new V1ObjectMeta(name: "iis-example"),
            //    spec: new V1PodSpec(
            //        containers: new List<V1Container>
            //                {
            //        new V1Container(
            //            image: "microsoft/iis:nanoserver",
            //            name: "iis",
            //            ports: new List<V1ContainerPort> { new V1ContainerPort(containerPort: 80) })
            //                })
            //);


            try
            {
                var resultPod = await client.client.CreateNamespacedPodAsync(pod, "default");
                //podList.AddOrUpdate
                //(name, new TerraformPodViewModel { Pod = pod, Logfile = logfile }, (s, pod) =>
                //{
                //    pod.Logfile = logfile;
                //    return pod;
                //});
            }
            catch (Exception e)
            {
                throw;
            }
            return name;
        }

    }
}
