using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace Kronstadt.Core.Logging;

internal delegate void InputCommited(string message);

internal class ThreadConsole : ICommandInputOutput
{
    private ILogger? _Logger;

    private Thread? _Thread;

    public event CommandInputHandler? inputCommitted;

    public void initialize(CommandWindow commandWindow)
    {
        // Blatantly ripped from OM
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CommandLineFlag? shouldManageConsole = null;
            bool previousShouldManageConsoleValue = true;

            Type windowsConsole = typeof(Provider).Assembly.GetType("SDG.Unturned.WindowsConsole");
            FieldInfo? shouldManageConsoleField = windowsConsole?.GetField("shouldManageConsole", BindingFlags.Static | BindingFlags.NonPublic);

            if (shouldManageConsoleField != null)
            {
                shouldManageConsole = (CommandLineFlag)shouldManageConsoleField.GetValue(null);
                previousShouldManageConsoleValue = shouldManageConsole.value;
                shouldManageConsole.value = false;
            }
        }

        commandWindow.title = "Kronstadt";
        _Logger = LoggerProvider.CreateLogger("SDG.Unturned");

        CommandWindow.shouldLogChat = false;
        CommandWindow.shouldLogDeaths = false;
        CommandWindow.shouldLogAnticheat = false;
        CommandWindow.shouldLogJoinLeave = false;

        Console.CancelKeyPress += OnCancelling;

        UTF8Encoding encoding = new UTF8Encoding(true);
        Console.OutputEncoding = encoding;
        Console.InputEncoding = encoding;

        _Thread = new(InputThreadLoop);
        _Thread.IsBackground = true;
        _Thread.Start();
    }

    public void outputError(string error)
    {
        _Logger?.LogError(error);
    }

    public void outputInformation(string information)
    {
        _Logger?.LogInformation(information);
    }

    public void outputWarning(string warning)
    {
        _Logger?.LogWarning(warning);
    }

    public void shutdown(CommandWindow commandWindow)
    {
    }

    public void update()
    {
    }

    private void OnCancelling(object sender, ConsoleCancelEventArgs e)
    {
        ServerManager.QueueShutdown(0);
        e.Cancel = true;
    }

    private void HandleInput() 
    {
        if (!Console.KeyAvailable)
        {
            return;
        }

        string text = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(text))
        {
            inputCommitted?.Invoke(text);
        }
    }

    private void InputThreadLoop() 
    {
        while (true) 
        {
            try
            {
                HandleInput();
                Thread.Sleep(50);
            }
            catch (Exception)
            {
            }
        }
    }
}
