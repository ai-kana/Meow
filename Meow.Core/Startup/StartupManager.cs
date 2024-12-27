using System.Reflection;
using System.Runtime.CompilerServices;

namespace Meow.Core.Startup;

public static class StartupManager
{
    public static void RunStartup(Assembly assembly)
    {
        IEnumerable<Type> types = assembly.GetTypes().Where(x => x.GetCustomAttribute<StartupAttribute>() != null);
        foreach (Type type in types)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}
