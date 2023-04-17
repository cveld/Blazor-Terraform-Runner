using BlazorTerraformRunner.Utility;
using ConsoleApp1;
using libVT100;
using Microsoft.AspNetCore.Components;
using System.Buffers;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BlazorTerraformRunner
{
    public class MonitorBase: ComponentBase, IAsyncDisposable
    {
        readonly static object lockobj = new object();
        [Parameter]
        public string? LogFile { get; set; }

        public string output = string.Empty;
        public string logging = string.Empty;
        public string errors = string.Empty;
        public bool isCompleted;
        public bool doneReading;
        Vt100Streamer? vt100Streamer;
        [Inject]
        Func<Vt100Streamer>? Vt100StreamerBuilder { get; set; }

        public MonitorBase()
        {
        }

        protected override void OnParametersSet()
        {
            output = "";
            ArgumentNullException.ThrowIfNull(Vt100StreamerBuilder);
            StateHasChanged();
            if (String.IsNullOrEmpty(LogFile))
            {
                return;
            }

            var initTask = (Task input) =>
            {
                // Set up a new process
                vt100Streamer = Vt100StreamerBuilder();
                output = "";

                vt100Streamer.StartStreaming(LogFile, AddStringToOutput);
            };

            System.Threading.Monitor.Enter(lockobj);

            // Check if the process is already running
            if (vt100Streamer != null)
            {
                vt100Streamer.Cancel().ContinueWith(initTask);
            }
            else
            {
                initTask(Task.CompletedTask);
            }
            
            System.Threading.Monitor.Exit(lockobj);
            return;
        }



        void AddStringToOutput()
        {
            ArgumentNullException.ThrowIfNull(vt100Streamer);
            output = vt100Streamer.htmlstream;
            InvokeAsync(StateHasChanged);
        }

        

        public async ValueTask DisposeAsync()
        {
            if (vt100Streamer != null)
            {
                await vt100Streamer.Cancel();
            }
        }
    }
}
