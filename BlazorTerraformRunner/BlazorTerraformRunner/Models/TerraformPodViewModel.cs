using k8s.Models;

namespace BlazorTerraformRunner.Models
{
    public class TerraformPodViewModel
    {
        public required V1Pod Pod { get; set; }
        public string? Logfile { get; set; }
        public Guid? Id { get; set; }
    }
}
