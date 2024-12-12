namespace Meow.Core.Commands.Framework;

internal interface IArgumentParser
{
    public Type Type {get;} 
    public bool TryParseArgument(IEnumerator<string> enumerator, out object? result);
}
