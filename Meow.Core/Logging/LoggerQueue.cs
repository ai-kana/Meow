using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Text;

namespace Meow.Core.Logging;

internal sealed class LoggerQueue : IDisposable
{
    private ConcurrentQueue<LogMessage> _Queue = new();
    private bool _IsWriting = false;
    private StreamWriter _FileWriter;
    private TextWriter _ConsoleWriter;

    public void Enqueue(LogMessage message)
    {
        if (_IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LoggerQueue));
        }

        _Queue.Enqueue(message);

        if (!_IsWriting)
        {
            _ = WriteAsync();
        }
    }

    private async Task WriteAsync()
    {
        _IsWriting = true;

        while (_Queue.TryDequeue(out LogMessage message))
        {
            await _FileWriter.WriteLineAsync(message.FileMessage);
            await _ConsoleWriter.WriteLineAsync(message.ConsoleMessage);
        }

        await _FileWriter.FlushAsync();
        await _ConsoleWriter.FlushAsync();

        _IsWriting = false;
    }

    private bool _IsDisposed = false;
    public void Dispose()
    {
        _IsDisposed = true;
        _FileWriter.Dispose();
    }

    internal LoggerQueue(StreamWriter writer)
    {
        _ConsoleWriter = Console.Out;
        _FileWriter = writer;
    }
}
