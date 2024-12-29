using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using Meow.Core.Bot;
using SDG.Unturned;

namespace Meow.Core.Logging;

internal sealed class LoggerQueue : IDisposable
{
    private StreamWriter _FileWriter;
    private TextWriter _ConsoleWriter;
    private bool _IsWriting = false;
    private ConcurrentQueue<LogMessage> _Queue;

    internal LoggerQueue(StreamWriter writer)
    {
        _ConsoleWriter = Console.Out;
        _FileWriter = writer;
        _Queue = new();
    }

    public void Enqueue(LogMessage message)
    {
        if (_IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LoggerQueue));
        }

        _Queue.Enqueue(message);
        if (_IsWriting)
        {
            return;
        }

        _IsWriting = true;
        WriteAsync().Forget();
    }

    private async UniTask WriteAsync()
    {
        while (_Queue.TryDequeue(out LogMessage message))
        {
            await _FileWriter.WriteLineAsync(message.FileMessage);
            await _ConsoleWriter.WriteLineAsync(message.ConsoleMessage);
            if (Level.isLoaded)
            {
                BotManager.LogQueue.Enqueue(message.FileMessage);
            }
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
}
