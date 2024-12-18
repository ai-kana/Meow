namespace Meow.Core.Translations;

public class TranslationList
{
    public static readonly Translation GreaterThanZero = new("GreaterThanZero");
    public static readonly Translation LessThanX = new("LessThanX");
    public static readonly Translation GreaterThanX = new("GreaterThanX");
    public static readonly Translation BadNumber = new("BadNumber");
    public static readonly Translation On = new("On");
    public static readonly Translation Off = new("Off");
    public static readonly Translation OneArgument = new("OneArgument");
    public static readonly Translation TwoArguments = new("TwoArguments");
    public static readonly Translation ThreeArguments = new("ThreeArguments");
    public static readonly Translation FourArguments = new("FourArguments");
    public static readonly Translation InvalidTime = new("InvalidTime");
    
    public static readonly Translation Second = new("Second");
    public static readonly Translation Seconds = new("Seconds");
    public static readonly Translation Minute = new("Minute");
    public static readonly Translation Minutes = new("Minutes");
    public static readonly Translation Hour = new("Hour");
    public static readonly Translation Hours = new("Hours");
    public static readonly Translation Day = new("Day");
    public static readonly Translation Days = new("Days");

    public static readonly Translation DayWord = new("Day");
    public static readonly Translation NightWord = new("Night");

    public static readonly Translation ShutdownMessage = new("Shutdown");
    public static readonly Translation ShutdownKick = new("ShutdownKick");
}
