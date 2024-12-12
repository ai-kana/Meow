namespace Meow.Core.Commands.Framework;
    
[AttributeUsage(AttributeTargets.Class)]
public class CommandParentAttribute : Attribute
{
    public readonly Type Owner;
    public CommandParentAttribute(Type owner)
    {
        Owner = owner;
    }
}
