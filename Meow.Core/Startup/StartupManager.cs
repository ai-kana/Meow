using System.Reflection;
using System.Runtime.CompilerServices;
using Meow.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Meow.Core.Startup;

public class StartupManager
{
    public static void RunStartup(Assembly assembly)
    {
        IEnumerable<Type> types = assembly.GetTypes().Where(x => x.GetCustomAttribute<StartupAttribute>() != null);
        ILogger logger = LoggerProvider.CreateLogger<StartupManager>();
        foreach (Type type in types)
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Failed to run start up on type {type.FullName}");
            }
        }
    }
}
