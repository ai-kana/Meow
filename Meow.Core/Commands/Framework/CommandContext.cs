using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.Framework;

public readonly struct CommandContext
{
    public readonly IPlayer Caller;
    private readonly IEnumerator<string> _Enumerator; 
    private readonly IEnumerable<string> _Arguments; 
    private readonly Type _Type;

    public CommandContext(Type type, IEnumerable<string> arguments, IPlayer caller)
    {
        _Type = type;
        _Arguments = arguments;
        _Enumerator = _Arguments.GetEnumerator();
        _Enumerator.MoveNext();
        Caller = caller;
    }

    public CommandExitedException Reply(object text)
    {
        Caller.SendMessage(text.ToString());
        return new();
    }

    public CommandExitedException Reply(string format, params object[] args)
    {
        Caller.SendMessage(format, args);
        return new();
    }

    public CommandExitedException Reply(Translation translation, params object[] args)
    {
        Caller.SendMessage(translation.Translate(Caller.Language, args));
        return new();
    }

    public CommandExitedException Exit => new();

    public bool HasArguments(int count)
    {
        return count <= _Arguments.Count();
    }

    public bool HasExactArguments(int count)
    {
        return count == _Arguments.Count();
    }

    public void AssertArguments(int count)
    {
        if (count > _Arguments.Count())
        {
            throw Reply(TranslationList.AssertArguments, count);
        }
    }

    public void AssertPermission(string permission)
    {
        if (Caller is not MeowPlayer player)
        {
            return;
        }

        if (!player.Permissions.HasPermission(permission))
        {
            throw Reply(TranslationList.AssertPermission);
        }
    }

    public void AssertPlayer()
    {
        if (Caller is not MeowPlayer)
        {
            throw Reply(TranslationList.AssertPlayer);
        }
    }

    public void AssertPlayer(out MeowPlayer caller)
    {
        if (Caller is not MeowPlayer player)
        {
            throw Reply(TranslationList.AssertPlayer);
        }

        caller = player;
    }

    public void AssertCooldown()
    {
        if (Caller is not MeowPlayer player)
        {
            return;
        }

        long time = player.Cooldowns.GetCooldown(_Type.FullName);
        if (time == 0)
        {
            return;
        }

        throw Reply(TranslationList.AssertCooldown, Formatter.FormatTime(time));
    }

    private static readonly Translation AssertDuty = new("AssertDuty", "You must be on duty to use this command");
    public void AssertOnDuty()
    {
        if (Caller is not MeowPlayer player)
        {
            return;
        }

        if (!player.Administration.OnDuty)
        {
            throw Reply(AssertDuty);
        }
    }

    private static readonly Translation AssertZone = new("AssertZone", "You cannot use that command here");
    public void AssertZoneFlag(string flag)
    {
        if (Caller is not MeowPlayer player)
        {
            return;
        }

        if (!player.Movement.HasZoneFlag(flag))
        {
            throw Reply(AssertZone);
        }
    }

    public void AddCooldown(long length)
    {
        if (Caller is not MeowPlayer player)
        {
            return;
        }

        player.Cooldowns.AddCooldown(_Type.FullName, length);
    }


    public bool MatchParameter(params string[] matches)
    {
        return matches.Contains(Current, StringComparer.OrdinalIgnoreCase);
    }

    public T Parse<T>()
    {
        return CommandParser.Parse<T>(_Enumerator);
    }

    public bool TryParse<T>(out T result)
    {
        return CommandParser.TryParse<T>(_Enumerator, out result);
    }

    public string Current => _Enumerator.Current;

    public bool MoveNext()
    {
        return _Enumerator.MoveNext();
    }

    public void Reset()
    {
        _Enumerator.Reset();
    }

    public string Form()
    {
        if (Current == null)
        {
            return string.Empty;
        }
        List<string> args = new();
        args.Add(Current);
        while (MoveNext())
        {
            args.Add(Current);
        }
        return string.Join(" ", args);
    }
}
