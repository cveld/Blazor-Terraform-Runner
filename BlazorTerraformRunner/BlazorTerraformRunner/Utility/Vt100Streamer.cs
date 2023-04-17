using ConsoleApp1;
using libVT100;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Buffers;
using System.ComponentModel;
using System.IO.Pipelines;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace BlazorTerraformRunner.Utility
{
    public class Vt100Streamer
    {
        bool doneReading;
        long latestPosition = 0;
        string? filePath;
        Action? handler;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken;
        public string htmlstream = String.Empty;
        Task? completion;

        public Vt100Streamer()
        {
            cancellationToken = cancellationTokenSource.Token;
        }

        public void StartStreaming(string filePath, Action handler)
        {
            this.filePath = filePath;
            this.handler = handler;
            completion = ReadLogLines();
        }

        public async Task Cancel()
        {
            ArgumentNullException.ThrowIfNull(nameof(completion));
            cancellationTokenSource.Cancel();
            try
            {
                await completion!;
            }
            catch (TaskCanceledException) { }
        }

        async Task ReadLogLines()
        {
            ArgumentNullException.ThrowIfNull(nameof(filePath));
            while (!cancellationToken.IsCancellationRequested)
            {

                var fileInfo = new FileInfo(filePath!);
                if (fileInfo.Exists && fileInfo.Length != latestPosition)
                {
                    var pipe = new Pipe();
                    Task<long> writing = FillPipeAsync(pipe.Writer, cancellationToken);
                    Task reading = ReadPipeAsync(pipe.Reader, cancellationToken);

                    var completion = Task.WhenAll(reading, writing);
                    await completion;
                    latestPosition = writing.Result;
                }
                await Task.Delay(5000, cancellationToken);
            }
        }

        async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(handler));
            //var sw = new StreamWriter(Console.OpenStandardOutput());
            while (true)
            {
                ReadResult result = await reader.ReadAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                ReadOnlySequence<byte> buffer = result.Buffer;
                foreach (ReadOnlyMemory<byte> mem in buffer)
                {
                    htmlstream += Encoding.UTF8.GetString(mem.Span);
                    handler!();
                }
                reader.AdvanceTo(buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();
        }



        public async Task<long> FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(filePath));
            var html = new HtmlAnsiDecoder(writer);
            IAnsiDecoder vt100 = new AnsiDecoder();
            string encoding = "ibm437";
            vt100.Encoding = Encoding.GetEncoding(encoding);
            vt100.Subscribe(html);

            doneReading = false;
            using Stream stream = File.Open(filePath!, FileMode.Open, FileAccess.Read);
            stream.Seek(latestPosition, SeekOrigin.Begin);
            
            // var sr = new StreamReader(stream, Encoding.);

            int read = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                read = stream.ReadByte();
                if (read == -1) { break; }
                vt100.Input(new byte[] { (byte)read });
            }
            latestPosition = stream.Position;
            await writer.FlushAsync();
            doneReading = true;
            writer.Complete();
            return latestPosition;
        }
    }
}
