namespace XProxy.Misc;

public static class NetworkingMessagesGenerator
{
    public static void Generate()
    {
        Dictionary<ushort, Type> types = ProxyUtils.FindNetworkMessageTypes();

        var sb = new StringBuilder();
        sb.AppendLine("public static class NetworkingMessages");
        sb.AppendLine("{");

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
