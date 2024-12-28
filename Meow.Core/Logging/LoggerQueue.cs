using Cysharp.Threading.Tasks;
using Meow.Core.Bot;
using SDG.Unturned;

namespace Meow.Core.Logging;

internal sealed class LoggerQueue : IDisposable
{
    private StreamWriter _FileWriter;
    private TextWriter _ConsoleWriter;

    public void Enqueue(LogMessage message)
    {
        if (_IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LoggerQueue));
        }

        WriteAsync(message).Forget();
    }

    private SemaphoreSlim _Semaphore = new(1, 1);
    private async UniTask WriteAsync(LogMessage message)
    {
        await _Semaphore.WaitAsync();

        await _FileWriter.WriteLineAsync(message.FileMessage);
        await _ConsoleWriter.WriteLineAsync(message.ConsoleMessage);
        if (Level.isLoaded)
        {
            BotManager.LogQueue.Enqueue(message.FileMessage);
        }

        _Semaphore.Release();
        if (_Semaphore.CurrentCount == 0)
        {
            await _FileWriter.FlushAsync();
            await _ConsoleWriter.FlushAsync();
        }
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
