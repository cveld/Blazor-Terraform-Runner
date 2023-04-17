using BlazorTerraformRunner.Models;
using System.Text.Json;

namespace BlazorTerraformRunner.Services
{
    public class WorkspaceConfigService
    {
        public Lazy<WorkspaceConfig> _currentWorkspaceConfig = new Lazy<WorkspaceConfig>(() =>
        {
            var jobFile = @"C:\Users\CarlintVeld\OneDrive - CloudNation\Projects\2022-11 Dotnet terraform\database\job1.json";
            string file = File.ReadAllTextAsync(jobFile).Result;
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var jobConfig = JsonSerializer.Deserialize<WorkspaceConfig>(file, options) ?? throw new Exception($"Cannot deserialize {jobFile} to JobConfig object");
            return jobConfig;
        });
        public WorkspaceConfig CurrentWorkspaceConfig { 
            get {
                return _currentWorkspaceConfig.Value;
            }
        }
    }
}
