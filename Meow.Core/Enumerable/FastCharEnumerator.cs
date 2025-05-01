using System.Collections;

namespace Meow.Core.Enumerable;

public struct FastCharEnumerator : IEnumerator<char>
{
    public FastCharEnumerator(string str)
    {
        _String = str;
        _Index = -1;
    }

    private int _Index;
    private readonly string _String;

    public char Current => _String[_Index];
    object IEnumerator.Current => _String[_Index];

    public bool MoveNext()
    {
        _Index++;
        return _Index < _String.Length;
    }

    public void Reset()
    {
        _Index = -1;
    }

    public void Dispose()
    {
    }
}
