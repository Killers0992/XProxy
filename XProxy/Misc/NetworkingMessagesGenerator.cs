using System.Reflection;

namespace XProxy.Misc;

public class NetworkingMessagesGenerator
{
    public struct CommandInfo
    {
        public string FullName;
        public string Name;
        public int FunctionHash;

        public CommandInfo(string fullName, string name, int hash)
        {
            FullName = fullName;
            Name = name;
            FunctionHash = hash;
        }
    }

    public static void Generate()
    {
        Dictionary<ushort, Type> types = ProxyUtils.FindNetworkMessageTypes();

        var sb = new StringBuilder();
        sb.AppendLine("public static class NetworkingMessages");
        sb.AppendLine("{");

        Type[] allTypes = typeof(ServerConsole).Assembly
            .GetTypes()
            .OrderBy(x => x.Name)
            .ToArray();

        Dictionary<Type, List<CommandInfo>> commands = new Dictionary<Type, List<CommandInfo>>();

        foreach(var type in allTypes)
        {
            string typeName = type.Name;

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach(var method in methods)
            {
                CommandAttribute cmd = method.GetCustomAttribute<CommandAttribute>();

                if (cmd == null)
                    continue;

                string cmdName = method.Name.Substring(3);
                string fullName = $"System.Void {type.FullName}::{method.Name}({(method.GetParameters() == null ? string.Empty : string.Join(", ", method.GetParameters().Select(x => $"System.{x.ParameterType.Name}")))})";
                int methodHash = fullName.GetStableHashCode();

                if (!commands.TryGetValue(type, out List<CommandInfo> cmds))
                {
                    cmds = new List<CommandInfo>();
                    commands.Add(type, cmds);
                }

                cmds.Add(new CommandInfo(fullName, cmdName, methodHash));
            }
        }

        foreach (var kvp in types.OrderBy(k => k.Key))
        {
            ushort id = kvp.Key;
            Type type = kvp.Value;

            string constName = MakeSafeName(type.Name);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {type.FullName}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public const ushort {constName} = {id};");
            sb.AppendLine();
        }


        foreach (var cmd in commands)
        {
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Class {cmd.Key.Name}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {cmd.Key.Name}");
            sb.AppendLine("    {");

            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// Commands.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        public class Commands");
            sb.AppendLine("        {");

            foreach (var info in cmd.Value)
            {
                sb.AppendLine("            /// <summary>");
                sb.AppendLine($"            /// Command {info.Name}");
                sb.AppendLine($"            /// {info.FullName}");
                sb.AppendLine("            /// </summary>");
                sb.AppendLine($"            public const ushort {info.Name} = {(ushort)info.FunctionHash};");
                sb.AppendLine();
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        File.WriteAllText("../../../Misc/NetworkingMessages.cs", sb.ToString());
        Console.WriteLine("NetworkingMessages.cs generated successfully.");
    }

    static string MakeSafeName(string typeName)
    {
        var safe = new StringBuilder();
        if (!char.IsLetter(typeName[0]) && typeName[0] != '_')
            safe.Append('_');

        foreach (char c in typeName)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                safe.Append(c);
            else
                safe.Append('_');
        }

        return safe.ToString();
    }
}
