using Steamworks;
using Kronstadt.Core.Formatting;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Bot;

internal readonly struct RconPlayer : IPlayer
{
    public string Name => "Console";

    public string LogName => $"Discord: {_Id.ToString()}";

    public CSteamID SteamID => new(_Id);

    public string Language => "English";

    private readonly ulong _Id;
    private readonly string _Command;

    public RconPlayer(ulong id, string command)
    {
        _Id = id;
        _Command = command;
    }

    public void SendMessage(string format, params object[] args)
    {
        BotManager.CommandReplies.Enqueue(new(_Id, Formatter.RemoveRichText(Formatter.Format(format, args))));
    }

    public void SendMessage(Translation translation, params object[] args)
    {
        BotManager.CommandReplies.Enqueue(new(_Id, Formatter.RemoveRichText(translation.Translate("English", args))));
    }
}
