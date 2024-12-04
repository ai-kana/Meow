namespace Kronstadt.Core.Logging;

internal struct LogMessage
{
    public readonly string ConsoleMessage;
    public readonly string FileMessage;

    public LogMessage(string consoleMessage, string fileMessage)
    {
        ConsoleMessage = consoleMessage;
        FileMessage = fileMessage;
    }
}
