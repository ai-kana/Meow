using Steamworks;

namespace Meow.Core.Offenses;

public class Offense
{
    public static Offense Create(OffenseType type, CSteamID offender, CSteamID issuer, string reason, long duration)
    {
        return new()
        {
            OffenseType = type,
            Offender = offender.m_SteamID,
            Issuer = issuer.m_SteamID,
            Issued = DateTimeOffset.Now.ToUnixTimeSeconds(),
            Duration = duration,
            Pardoned = false,
            Reason = reason,
        };
    }

    public int Id {get; set;} 
    public OffenseType OffenseType {get; set;}
    public ulong Offender {get; set;} 
    public ulong Issuer {get; set;} 
    public long Issued {get; set;} 
    public long Duration {get; set;} 
    public bool Pardoned {get; set;}
    private const string DefaultReason = "No reason provided";
    public string Reason {get; set;} = DefaultReason;

    public bool IsActive => IsPermanent ? true : (Duration + Issued) > DateTimeOffset.Now.ToUnixTimeSeconds();
    public long Remaining => !IsActive ? 0 : (Duration + Issued) - DateTimeOffset.Now.ToUnixTimeSeconds();
    public bool IsPermanent => Pardoned ? false : Duration == 0;
}
