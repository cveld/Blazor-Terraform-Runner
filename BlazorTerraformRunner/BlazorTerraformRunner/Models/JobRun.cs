namespace BlazorTerraformRunner.Models
{
    public class JobRun
    {
        public string? Command { get; set; }
        public Guid Id { get; set; }
        public string? LogFile { get; set; }
        public string? PodName { get; set; }
        public string? PodStatus { get; set; }
    }
}
