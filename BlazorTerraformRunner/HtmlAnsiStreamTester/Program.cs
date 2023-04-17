// See https://aka.ms/new-console-template for more information
using libVT100;
using System.Text;
using ConsoleApp1;
using System.Buffers;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BlazorTerraformRunner.Utility;
using System.Threading;

//void Test1()
//{
//    string encoding = "ibm437";
//    System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
//    Encoding.RegisterProvider(provider);

//    var inputFilename = @"C:\work\git\github\cveld\Experiments\2022-11 Blazor terraform runner\coloroutput.txt";
//    var outputFilename = @"C:\work\git\github\cveld\Experiments\2022-11 Blazor terraform runner\coloroutput.html";
//    IAnsiDecoder vt100 = new AnsiDecoder();
//    var html = new HtmlAnsiDecoder();
//    vt100.Encoding = Encoding.GetEncoding(encoding);
//    vt100.Subscribe(html);

//    using Stream stream = File.Open(inputFilename, FileMode.Open);
//    var sr = new StreamReader(stream);

//    var sr_html = new StreamReader(html.output);

//    int read = 0;
//    while ((read = sr.Read()) != -1)
//    {
//        vt100.Input(new byte[] { (byte)read });
//        while ((read = sr_html.Read()) != -1)
//        {
//            var c = (char)read;
//            Console.Write(c);
//        }
//    }
////File.WriteAllText(outputFilename, html.html);
//}

string output = "";
await Test3();

async Task Test3()
{
    System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
    Encoding.RegisterProvider(provider);
    Vt100Streamer vt100Streamer = new Vt100Streamer();
    var inputFilename = @"C:\Users\CarlintVeld\OneDrive - CloudNation\Projects\2022-11 Dotnet terraform\logtests\utf8.log";
    var pipe = new Pipe();
    Task<long> writing = vt100Streamer.FillPipeAsync(0, inputFilename, pipe.Writer, CancellationToken.None);
    Task reading = ReadPipeAsync(pipe.Reader);

    var completion = Task.WhenAll(reading, writing);
    await completion;
    Console.WriteLine(output);

}

async Task Test2()
{
    string encoding = "ibm437";
    System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
    Encoding.RegisterProvider(provider);

    var inputFilename = @"C:\work\git\github\cveld\Experiments\2022-11 Blazor terraform runner\coloroutput.txt";
    inputFilename = @"C:\temp\terraform\20221120214014_3732d8cd-40b3-4e0a-8d96-64a4f626f6f2.log";
    inputFilename = @"C:\Users\CarlintVeld\OneDrive - CloudNation\Projects\2022-11 Dotnet terraform\logtests\utf8.log";
    var outputFilename = @"C:\work\git\github\cveld\Experiments\2022-11 Blazor terraform runner\coloroutput.html";
    IAnsiDecoder vt100 = new AnsiDecoder();

    var pipe = new Pipe();

    var html = new HtmlAnsiDecoder(pipe.Writer);
    vt100.Encoding = Encoding.GetEncoding(encoding);
    vt100.Subscribe(html);

    Task writing = FillPipeAsync2(inputFilename, vt100, pipe.Writer);
    //Task writing = FillPipeAsync(inputFilename, pipe.Writer);
    Task reading = ReadPipeAsync(pipe.Reader);

    await Task.WhenAll(reading, writing);

    Console.WriteLine(output);

    
}

async Task FillPipeAsync(string inputFilename, PipeWriter writer)
{
    using Stream stream = File.Open(inputFilename, FileMode.Open);
    var sr = new StreamReader(stream);

    int read = 0;
    while ((read = sr.Read()) != -1)
    {
        var written = (int)Encoding.UTF8.GetBytes(new char[] { (char)read }, writer);
        //writer.Advance(written);
        await writer.FlushAsync();
    }
    writer.Complete();
}

async Task FillPipeAsync2(string inputFilename, IAnsiDecoder vt100, PipeWriter writer)
{
    using Stream stream = File.Open(inputFilename, FileMode.Open);
    var sr = new StreamReader(stream);

    int read = 0;
    while ((read = sr.Read()) != -1)
    {
        vt100.Input(new byte[] { (byte)read });
    }

    writer.Complete();
}



async Task ReadPipeAsync(PipeReader reader)
{
    //var sw = new StreamWriter(Console.OpenStandardOutput());

    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;
        output += Encoding.UTF8.GetString(buffer);
        //foreach (ReadOnlyMemory<byte> mem in buffer)
        //{
        //    output += Encoding.UTF8.GetString(mem.Span);
        //}
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

async Task ProcessLinesAsync(Socket socket)
{
    var pipe = new Pipe();
    Task writing = FillPipeAsync_example(socket, pipe.Writer);
    Task reading = ReadPipeAsync_example(pipe.Reader);

    await Task.WhenAll(reading, writing);
}

async Task FillPipeAsync_example(Socket socket, PipeWriter writer)
{
    const int minimumBufferSize = 512;

    while (true)
    {
        // Allocate at least 512 bytes from the PipeWriter.
        Memory<byte> memory = writer.GetMemory(minimumBufferSize);
        try
        {
            int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
            if (bytesRead == 0)
            {
                break;
            }
            // Tell the PipeWriter how much was read from the Socket.
            writer.Advance(bytesRead);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            break;
        }

        // Make the data available to the PipeReader.
        FlushResult result = await writer.FlushAsync();

        if (result.IsCompleted)
        {
            break;
        }
    }

    // By completing PipeWriter, tell the PipeReader that there's no more data coming.
    await writer.CompleteAsync();
}

async Task ReadPipeAsync_example(PipeReader reader)
{
    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;

        while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
        {
            // Process the line.
            Console.WriteLine(line.Length);
        }

        // Tell the PipeReader how much of the buffer has been consumed.
        reader.AdvanceTo(buffer.Start, buffer.End);

        // Stop reading if there's no more data coming.
        if (result.IsCompleted)
        {
            break;
        }
    }

    // Mark the PipeReader as complete.
    await reader.CompleteAsync();
}

bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
{
    // Look for a EOL in the buffer.
    SequencePosition? position = buffer.PositionOf((byte)'\n');

    if (position == null)
    {
        line = default;
        return false;
    }

    // Skip the line + the \n.
    line = buffer.Slice(0, position.Value);
    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
    return true;
}
