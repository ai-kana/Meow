namespace Meow.Core.Commands.Framework;

[AttributeUsage(AttributeTargets.Class)]
public class CommandSyntaxAttribute : Attribute
{
    public readonly string Syntax;

    public CommandSyntaxAttribute(string syntax)
    {
        Syntax = syntax;
    }
}
