namespace BlazorTerraformRunner.Models
{
    public class WorkspaceConfig
    {
        public string? RootModule { get; set; }
        public string? RootFolderLocal { get; set; }
        public string? RootFolderContainer { get; set; }
        public string? LockFileSource { get; set; }

        public string? DataFolder { get; set; }
        public string? LogFolder { get; set; }
        public string? State { get; set; }
        public string? BackendConfig { get; set; }
        public string? AzureCli { get; set; }
        public string JobName { get; set; }
        public List<string>? IgnoreFiles { get; set; }

    }


}
