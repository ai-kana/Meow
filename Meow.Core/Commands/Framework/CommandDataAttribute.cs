namespace Meow.Core.Commands.Framework;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandDataAttribute : Attribute
{
    public readonly string Name; 
    public readonly string[] Aliases; 

    public CommandDataAttribute(string name, params string[] aliases)
    {
        Name = name;
        Aliases = aliases;
    }
}
