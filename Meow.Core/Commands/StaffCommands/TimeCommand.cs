using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("time", "t")]
[CommandSyntax("[<Switches: get, set>]")]
internal class TimeCommand : Command
{
    public TimeCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertOnDuty();
        Context.AssertPermission("time");
        throw Context.Reply("[<Switches: get, set>]");
    }
}

[CommandParent(typeof(TimeCommand))]
[CommandData("get")]
internal class TimeGetCommand : Command
{
    public TimeGetCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation CurrentTime = new("CurrentTime");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("time");
        Context.AssertOnDuty();
        
        byte hour = (byte)(LightingManager.time / 150);
        byte minutes = (byte)((LightingManager.time % 150) / 2.5);
        
        // Offsetting time by 6 hours, so that when LightingManager.time is 0, it will show as 6:00.
        hour += 6;
        
        if (hour > 23) hour -= 24;
        
        string time = $"{hour:00}:{minutes:00}";

        throw Context.Reply(CurrentTime, time, LightingManager.time);
    }
}

[CommandParent(typeof(TimeCommand))]
[CommandData("set")]
[CommandSyntax("[<Params: time>]")]
internal class TimeSetCommand : Command
{
    public TimeSetCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation TimeSet = new("TimeSet");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("time");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        uint time = Context.Parse<uint>();
        
        LightingManager.time = time;
        throw Context.Reply(TimeSet, time);       
    }
}

[CommandParent(typeof(TimeSetCommand))]
[CommandData("day")]
internal class TimeDayCommand : Command
{
    public TimeDayCommand(CommandContext context) : base(context)
    {
    }

    public static readonly Translation TimeSetDayOrNight = new("TimeSetDayOrNight");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("time");
        Context.AssertOnDuty();

        LightingManager.time = (uint)(LightingManager.cycle * LevelLighting.transition);
        throw Context.Reply(TimeSetDayOrNight, new TranslationPackage(TranslationList.Day));
    }
}

[CommandParent(typeof(TimeSetCommand))]
[CommandData("night")]
internal class TimeNightCommand : Command
{
    public TimeNightCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("time");
        Context.AssertOnDuty();

        LightingManager.time = (uint)(LightingManager.cycle * (LevelLighting.bias + LevelLighting.transition));
        throw Context.Reply(TimeDayCommand.TimeSetDayOrNight, new TranslationPackage(TranslationList.NightWord));
    }
}
