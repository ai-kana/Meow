using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Meow.Core.Json;

public readonly struct JsonStreamWriter : IDisposable 
{
    private readonly StreamWriter _Writer;
    public JsonStreamWriter(Stream stream)
    {
        _Writer = new(stream);
    }

    public async UniTask WriteObject(object ob)
    {
        string content = JsonConvert.SerializeObject(ob);
        await _Writer.WriteAsync(content);
    }

    public void Dispose()
    {
        _Writer.Dispose();
    }
}
