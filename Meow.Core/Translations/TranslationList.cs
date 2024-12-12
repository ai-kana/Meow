namespace Meow.Core.Translations;

public class TranslationList
{
    // Generic Translations
    public static readonly Translation GreaterThanZero = new("GreaterThanZero", "Amount Must Be Greater Than Zero");
    public static readonly Translation LessThanX = new("LessThanX", "Amount must be less than {0}");
    public static readonly Translation GreaterThanX = new("GreaterThanX", "Amount must be greater than {0}");
    public static readonly Translation BadNumber = new("BadNumber", "Invalid Number");
    public static readonly Translation On = new("On", "on");
    public static readonly Translation Off = new("Off", "off");
    public static readonly Translation OneArgument = new("OneArgument", "{0}");
    public static readonly Translation TwoArguments = new("TwoArguments", "{0} {1}");
    public static readonly Translation ThreeArguments = new("ThreeArguments", "{0} {1} {2}");
    public static readonly Translation FourArguments = new("FourArguments", "{0} {1} {2} {3}");
    public static readonly Translation InvalidTime = new("InvalidTime", "Invalid Time");
    
    // Clear
    public static readonly Translation ClearedInventoryOther = new("ClearedInventoryOther", "Cleared {0}'s inventory");
    public static readonly Translation ClearedInventorySelf = new("ClearedInventorySelf", "Cleared your inventory");
    public static readonly Translation FailedToClearInventorySelf = new("FailedToClearInventorySelf", "Failed to clear inventory");
    public static readonly Translation FailedToClearInventoryOther = new("FailedToClearInventoryOther", "Failed to clear {0}'s inventory");
    public static readonly Translation ClearedGroundDistance = new("ClearedGroundDistance", "Cleared items within {0} meters");
    public static readonly Translation ClearedGround = new("ClearedGround", "Cleared ground");
    
    // Duty
    public static readonly Translation DutyStateGlobal = new("DutyStateGlobal", "{0} is now {1} duty");
    public static readonly Translation DutyStateSilent = new("DutyStateSilent", "You are now {0} duty");
    public static readonly Translation DutyStateCheck = new("DutyStateCheck", "You are {0} duty");
    
    // Experience
    public static readonly Translation RemovedExperience = new("RemovedExperience", "Removed {0} experience from {1}");
    public static readonly Translation SetExperience = new("SetExperience", "Set {0}'s experience to {1}");
    public static readonly Translation AddedExperience = new("AddedExperience", "Added {0} experience to {1}");
    public static readonly Translation CheckedExperience = new("CheckedExperience", "{0} has {1} experience");
    public static readonly Translation ResetExperience = new("ResetExperience", "Reset {0}'s experience");
    
    // Flag
    public static readonly Translation FlagDoesNotExist = new("FlagDoesNotExist", "Player {0} does not have flag {1}");
    public static readonly Translation FlagSet = new("FlagSet", "Set flag {0} for {1} to {2}");
    public static readonly Translation FlagGet = new("FlagGet", "Flag {0} for {1} is {2}");
    public static readonly Translation FlagUnset = new("FlagUnset", "Unset flag {0} for {1}");
    
    // Freeze
    public static readonly Translation PlayerAlreadyFrozen = new("PlayerAlreadyFrozen", "Player {0} is already frozen");
    public static readonly Translation PlayerFrozen = new("PlayerFrozen", "Successfully froze {0}");
    
    // GiveItem
    public static readonly Translation ItemNotFound = new("ItemNotFound", "Item not found");
    public static readonly Translation GaveItem = new("GaveItem", "You gave {0} {1} ({2})");
    public static readonly Translation GaveItemAmount = new("GaveItemAmount", "You gave {0} {1}x {2} ({3})");
    
    // God
    public static readonly Translation GodModeSelf = new("GodModeSelf", "God mode has been turned {0}");
    public static readonly Translation GodModeOther = new("GodModeOther", "{0}'s god mode is now {1}");
    
    // Heal
    public static readonly Translation HealedOther = new("HealedOther", "Successfully healed {0}");
    public static readonly Translation HealedSelf = new("HealedSelf", "Successfully healed yourself");
    
    // Item
    public static readonly Translation ItemSelfAmount = new("ItemSelfAmount", "You have received {0}x {1} ({2})");
    public static readonly Translation ItemSelf = new("ItemSelf", "You have received {0} ({1})");
    
    // Kick
    public static readonly Translation Kicked = new("Kicked", "Kicked {0}");
    public static readonly Translation KickedReason = new("KickedReason", "Kicked {0} for {1}");
    
    // Kill
    public static readonly Translation Killed = new("Killed", "Killed {0}");
    
    // Movement
    public static readonly Translation SetSpeedOther = new("SetSpeedOther", "Set {0}'s movement speed to {1}");
    public static readonly Translation SetSpeedSelf = new("SetSpeedSelf", "Set your movement speed to {0}");
    public static readonly Translation SetGravityOther = new("SetGravityOther", "Set {0}'s gravity to {1}");
    public static readonly Translation SetGravitySelf = new("SetGravitySelf", "Set your gravity to {0}");
    public static readonly Translation SetJumpOther = new("SetJumpOther", "Set {0}'s jump height to {1}");
    public static readonly Translation SetJumpSelf = new("SetJumpSelf", "Set your jump height to {0}");
    
    // Position
    public static readonly Translation PositionSelf = new("PositionSelf", "You are at: {0} | {1} | {2}");
    public static readonly Translation PositionTarget = new("PositionTarget", "{0} is at: {1} | {2} | {3}");
    
    // Private Message
    public static readonly Translation PrivateMessageSelf = new("PrivateMessageSelf", "You can't send a private message to yourself.");
    
    // Reply
    public static readonly Translation NoOneToReplyTo = new("NoOneToReplyTo", "You have no one to reply to");
    public static readonly Translation PlayerNotOnline = new("PlayerNotOnline", "The player you are trying to reply to is not online");
    
    // Reputation
    public static readonly Translation TookReputation = new("TookReputation", "Took {0} reputation from {1}");
    public static readonly Translation SetReputation = new("SetReputation", "Set {0}'s reputation to {1}");
    public static readonly Translation AddedReputation = new("AddedReputation", "Added {0} reputation to {1}");
    public static readonly Translation CheckedReputation = new("CheckedReputation", "{0} has {1} reputation");
    public static readonly Translation ResetReputation = new("ResetReputation", "Reset {0}'s reputation");
    
    // Role
    public static readonly Translation AddedRole = new("AddedRole", "Added {0} to {1}");
    public static readonly Translation RemovedRole = new("RemovedRole", "Removed {0} from {1}");
    public static readonly Translation DoesNotHaveRole = new("DoesNotHaveRole", "{0} does not have {1}");
    public static readonly Translation RoleList = new("RoleList", "Roles: {0}");
    public static readonly Translation RoleHasRole = new("RoleHasRole", "{0} has {1}");
    
    // Shutdown
    public static readonly Translation Shutdown = new("Shutdown", "Server is shutting down in {0}");
    public static readonly Translation ShutdownKick = new("ShutdownKick", "The server has shutdown");
    public static readonly Translation ShutdownCancelled = new("ShutdownCancelled", "Server shutdown cancelled");
    public static readonly Translation ShutdownNotActive = new("ShutdownNotActive", "Server is not shutting down");
    
    // Spy
    public static readonly Translation SpyingOn = new("SpyingOn", "Spying on {0}");
    
    // Teleport
    public static readonly Translation TeleportedToOther = new("TeleportedToOther", "Teleported you to {0}");
    public static readonly Translation TeleportedOtherToOther = new("TeleportedOtherToOther", "Teleported {0} to {1}");
    public static readonly Translation TeleportingToXYZ = new("TeleportingToXYZ", "Teleporting you to: {0} | {1} | {2}");
    public static readonly Translation TeleportingOtherToXYZ = new("TeleportingOtherToXYZ", "Teleporting {0} to: {1} | {2} | {3}");
    public static readonly Translation TeleportingToWaypoint = new("TeleportingToWaypoint", "Teleporting you to waypoint");
    public static readonly Translation NoWaypoint = new("NoWaypoint", "No waypoint set");
    public static readonly Translation TeleportingPlayerHere = new("TeleportingPlayerHere", "Teleporting {0} to you");
    
    // Time
    public static readonly Translation CurrentTime = new("CurrentTime", "Current time is {0} ({1})");
    public static readonly Translation TimeSet = new("TimeSet", "Time set to {0}");
    public static readonly Translation TimeSetDayOrNight = new("TimeSetDayOrNight", "Time set to {0}");
    public static readonly Translation DayWord = new("Day", "Day");
    public static readonly Translation NightWord = new("Night", "Night");
    
    // Unfreeze
    public static readonly Translation PlayerNotFrozen = new("PlayerAlreadyUnfrozen", "Player {0} is not frozen");
    public static readonly Translation PlayerUnfrozen = new("PlayerUnfrozen", "Successfully unfroze {0}");
    
    // Vehicle
    public static readonly Translation VehicleNotFound = new("VehicleNotFound", "Vehicle not found");
    public static readonly Translation SpawningVehicle = new("SpawningVehicle", "Spawning {0}");
    
    // Warn
    public static readonly Translation WarnedReason = new("WarnedReason", "Warned {0} for {1}");
    public static readonly Translation PardonedWarn = new("PardonedWarn", "Pardoned warning #{0}");
    public static readonly Translation HasNoWarns = new("HasNoWarns", "{0} has no warnings.");
    public static readonly Translation WarningListed = new("WarningListed", "Warning #{0} issued on {1} for {2}");
    
    // Ban
    public static readonly Translation CmdBanned = new("Banned", "Banned {0}");
    public static readonly Translation CmdBannedTime = new("BannedTime", "Banned {0} for {1}");
    public static readonly Translation CmdBannedTimeReason = new("BannedTimeReason", "Banned {0} for {1} for {2}");
    public static readonly Translation CmdBannedReason = new("BannedReason", "Banned {0} for {1}");
    
    // Offenses
    public static readonly Translation NoOffenses = new("NoOffenses", "{0} has no offenses.");
    public static readonly Translation Offense = new("Offense", "Offense #{0} issued for {1}");
    public static readonly Translation OffenseList = new("OffenseList", "Offenses for {0}");
    public static readonly Translation OffenseType = new("OffenseType", "Listing {0} for {1}");

    public static readonly Translation Bans = new("Bans", "Bans");
    public static readonly Translation Mutes = new("Mutes", "Mutes");
    public static readonly Translation Warns = new("Warns", "Warns");
    
    // Connection
    public static readonly Translation PlayerConnected = new("PlayerConnected", "{0} has joined the server");
    public static readonly Translation PlayerDisconnected = new("PlayerDisconnected", "{0} has left the server");

    // Command Manager
    public static readonly Translation NoCommandFound = new("NoCommandFound", "There is no command called {0}");

    // Command Context
    public static readonly Translation AssertArguments = new("AssertArguments", "This command requires {0} arguments");
    public static readonly Translation AssertPermission = new("AssertPermission", "You do not have permission to execute this command");
    public static readonly Translation AssertPlayer = new("AssertPlayer", "This command can only be executed by players");
    public static readonly Translation AssertCooldown = new("AssertCooldown", "You cannot use this command for {0}");

    // Time
    public static readonly Translation Second = new("Second", "{0} second");
    public static readonly Translation Seconds = new("Seconds", "{0} seconds");
    public static readonly Translation Minute = new("Minute", "{0} minute");
    public static readonly Translation Minutes = new("Minutes", "{0} minutes");
    public static readonly Translation Hour = new("Hour", "{0} hour");
    public static readonly Translation Hours = new("Hours", "{0} hours");
    public static readonly Translation Day = new("Day", "{0} day");
    public static readonly Translation Days = new("Days", "{0} days");

    // Banned
    public static readonly Translation BanPermanent = new("BanPermanent", "You are permanently banned for '{0}' appeal at {1}");
    public static readonly Translation BanTemporary = new("BanTemporary", "You are banned for '{0}' for another {1} appeal at {2}");

    // Muted
    public static readonly Translation MutePermanent = new("MutePermanent", "You are permanently muted for '{0}' appeal at {1}");
    public static readonly Translation MuteTemporary = new("MuteTemporary", "You are muted for '{0}' for another {1} appeal at {2}");
    public static readonly Translation Unmuted = new("Unmuted", "Your mute has expired");
}
