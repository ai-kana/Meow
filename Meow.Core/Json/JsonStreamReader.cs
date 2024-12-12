using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Meow.Core.Json;

public readonly struct JsonStreamReader : IDisposable {
    private readonly StreamReader _Reader;
    public JsonStreamReader(Stream stream)
    {
        _Reader = new(stream);
    }

    public async UniTask<T> ReadObject<T>()
    {
        string content = await _Reader.ReadToEndAsync();
        T? ret = JsonConvert.DeserializeObject<T>(content);
        if (ret == null)
        {
            throw new NullReferenceException();
        }

        return ret;
    }

    public void Dispose()
    {
        _Reader.Dispose();
    }
}
