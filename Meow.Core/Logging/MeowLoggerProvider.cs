using Microsoft.Extensions.Logging;

namespace Meow.Core.Logging;

internal sealed class MeowLoggerProvider : ILoggerProvider
{
    private readonly string FileName;
    private readonly string FileExtension;
    private readonly string FilePath;
    private string FullPath => $"{FilePath}/{FileName}.{FileExtension}";

    private LoggerQueue _Queue;

    internal MeowLoggerProvider(string logPath)
    {
        FileExtension = Path.GetFileName(logPath).Split('.').Last();
        FileName = Path.GetFileNameWithoutExtension(logPath);
        FilePath = Path.GetDirectoryName(logPath);
        
        Directory.CreateDirectory(FilePath);

        if (File.Exists(FullPath))
        {
            SaveFile();
        }

        StreamWriter writer = new(File.Create(FullPath));
        _Queue = new(writer);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MeowLogger(categoryName, _Queue);
    }

    public void SaveFile()
    {
        string time = DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt");
        File.Copy(FullPath, $"{FilePath}/{time}-{FileName}.{FileExtension}");
    }

    public void Dispose()
    {
        _Queue.Dispose();
        SaveFile();
    }
}
