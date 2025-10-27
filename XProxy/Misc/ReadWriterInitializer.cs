using System.Reflection;
using UnityEngine;

namespace XProxy.Misc;

public class ReadWriterInitializer
{
    public static void InitializeAll()
    {
        try
        {
            int count = 0;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Skip dynamic assemblies like "System.Reflection.Emit"
                if (assembly.IsDynamic)
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m =>
                            m.Name == "InitReadWriters" &&
                            m.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), inherit: false));

                    foreach (var method in methods)
                    {
                        try
                        {
                            method.Invoke(null, null);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            ProxyLogger.Error($"Failed to call {type.FullName}.{method.Name}(): {ex}");
                        }
                    }
                }
            }

            ProxyLogger.Error($"Initialized {count} read/writer method(s).", "InitReadWriters");
        }
        catch (Exception ex)
        {
            ProxyLogger.Error($"Initialization failed: {ex}", "InitReadWriters");
        }
    }
}
