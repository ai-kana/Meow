namespace Meow.Core.Commands.Framework;

public abstract class ArgumentParser<T> : IArgumentParser
{
    public Type Type => typeof(T);
    public abstract bool TryParse(IEnumerator<string> enumerator, out T result);

    public bool TryParseArgument(IEnumerator<string> enumerator, out object? result)
    {
        bool ret = TryParse(enumerator, out T r );
        result = r;
        return ret;
    }
}
