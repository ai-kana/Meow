using Steamworks;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Players;

public interface IPlayer
{
    public string Name {get;}
    public string LogName {get;}
    public CSteamID SteamID {get;}
    public string Language {get;}
    public void SendMessage(string format, params object[] args);
    public void SendMessage(Translation translation, params object[] args);
}
