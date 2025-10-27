using Hazards;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using Mirror;
using PlayerRoles.PlayableScps.HumanTracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XProxy.Validator;

public class MainClass : Plugin
{
    private static readonly Regex sWhitespace = new Regex(@"\s+");

    public override string Name { get; } = "XProxy.Validator";

    public override string Description { get; } = "Validator for XProxy which validates various aspects of XProxy functionality.";

    public override string Author { get; } = "Killers0992";

    public override Version Version { get; } = new Version(1, 0, 0);

    public override Version RequiredApiVersion { get; } = new Version(LabApi.Features.LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
    }

    public static string ReplaceWhitespace(string input, string replacement)
    {
        return sWhitespace.Replace(input, replacement);
    }

    private void OnWaitingForPlayers()
    {
        string generatedPath = $"D:\\VS Projects\\XProxy\\XProxy.API\\Networking";

        string componentsPath = Path.Combine(generatedPath, "Components");

        string objectsPath = Path.Combine(generatedPath, "Objects");

        if (!Directory.Exists(generatedPath))
            Directory.CreateDirectory(generatedPath);

        if (Directory.Exists(componentsPath))
            Directory.Delete(componentsPath, true);

        Directory.CreateDirectory(componentsPath);

        if (Directory.Exists(objectsPath))
            Directory.Delete(objectsPath, true);

        Directory.CreateDirectory(objectsPath);

        var identities = UnityEngine.Object.FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            // Only include scene objects.
            .Where(x => x.sceneId != 0)
            .ToList();

        foreach (var prefab in NetworkClient.prefabs)
        {
            if (!prefab.Value.TryGetComponent(out NetworkIdentity identity))
                continue;

            if (!identities.Contains(identity))
                identities.Add(identity);
        }

        foreach (NetworkIdentity identity in identities)
        {
            string className = CapitalizeFirst(identity.gameObject.name);

            className = ReplaceWhitespace(className, string.Empty);
            className = Regex.Replace(className, "[^a-zA-Z0-9]", "");
            className += "Object";

            GenerateObjectClass(generatedPath, className, identity);
        }
    }

    private static readonly Dictionary<Type, string> Aliases =
    new Dictionary<Type, string>()
    {
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(object), "object" },
        { typeof(bool), "bool" },
        { typeof(char), "char" },
        { typeof(string), "string" },
        { typeof(void), "void" },

        // From C# 11 onwards
        { typeof(nint), "nint" },
        { typeof(nuint), "nuint" },
    };

    public class ComponentGenerationInfo
    {
        public List<CustomPropertyInfo> ExtraProperties { get; set; } = new();
        public List<CustomSerializationInfo> SerializationHooks { get; set; } = new();
    }

    public class CustomPropertyInfo
    {
        public string TypeName { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public bool IsSyncVar { get; set; } = true; // false = not dirty-bit tracked
    }

    public class CustomSerializationInfo
    {
        public string MethodName { get; set; } = "";
        public bool RunOnInitialOnly { get; set; } = false;
        public List<string> WriteCalls { get; set; } = new();
    }

    Dictionary<string, ComponentGenerationInfo> componentMap = new()
    {
        ["AdminToys.WaypointToy"] = new ComponentGenerationInfo
        {
            ExtraProperties = new()
            {
                new CustomPropertyInfo
                {
                    TypeName = "byte",
                    PropertyName = "WaypointId",
                    IsSyncVar = false
                }
            },

            SerializationHooks = new()
            {
                new CustomSerializationInfo
                {
                    MethodName = "BeforeSerialize",
                    RunOnInitialOnly = false,
                    WriteCalls = new() { "writer.WriteULong(SyncVarDirtyBits);" }
                },
                new CustomSerializationInfo
                {
                    MethodName = "AfterSerialize",
                    RunOnInitialOnly = true,
                    WriteCalls = new() { "writer.WriteUInt(0);", "writer.WriteByte(WaypointId);" }
                }
            }
        },
        ["AdminToys.TextToy"] = new ComponentGenerationInfo
        {
            SerializationHooks = new()
            {
                new CustomSerializationInfo
                {
                    MethodName = "BeforeSerialize",
                    RunOnInitialOnly = false,
                    WriteCalls = new() { "writer.WriteULong(SyncVarDirtyBits);" }
                },
                new CustomSerializationInfo
                {
                    MethodName = "AfterSerialize",
                    RunOnInitialOnly = true,
                    WriteCalls = new() { "writer.WriteUInt(0);" }
                }
            }
        }
    };


    public class BehaviourInfo
    {
        public string Name { get; set; }

        public string NormalName => CapitalizeFirst(ReplaceWhitespace(Name, string.Empty));
        public string ClassName => $"{NormalName}Component";

        public Type BehaviourType { get; set; }
        public List<SyncVarInfo> SyncVars { get; set; } = new List<SyncVarInfo>();
        public List<SyncListInfo> SyncLists { get; set; } = new List<SyncListInfo>();
    }

    public class SyncVarInfo
    {
        public string Name { get; set; }

        public string NormalName
        {
            get
            {
                string final = ReplaceWhitespace(Name, string.Empty);

                if (final.StartsWith("_"))
                    final = final.Substring(1);

                return CapitalizeFirst(final);
            }
        }

        public string PrivateName => $"_{LowerFirst(NormalName)}";

        public string ValueName
        {
            get
            {
                string name = ValueType.Name;

                if (Aliases.TryGetValue(ValueType, out string alias))
                    return alias;

                return name;
            }
        }

        public Type ValueType { get; set; }
        public ulong Bit { get; set; }

        public string WriterName
        {
            get
            {
                string name = FindWriterMethodName(ValueType, out _);

                // Invalid
                if (name.StartsWith("_"))
                    name = "Write";

                return name;
            }
        }

        public string WriterNamespace
        {
            get
            {
                string nameSpace;
                FindWriterMethodName(ValueType, out nameSpace);
                return nameSpace;
            }
        }

        public string ReaderName => FindReaderMethodName(ValueType);
    }

    public class SyncListInfo
    {
        public Type Type { get; set; }
    }

    public static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    public static string LowerFirst(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }

    private void GenerateObjectClass(string path, string className, NetworkIdentity identity)
    {
        string objectsPath = Path.Combine(path, "Objects");

        string targetPath = Path.Combine(objectsPath, className + ".cs");

        if (File.Exists(targetPath))
            return;

        StringBuilder sb = new StringBuilder();

        NetworkBehaviour[] behaviours = identity.GetComponentsInChildren<NetworkBehaviour>(includeInactive: true);

        List<BehaviourInfo> behaviourInfos = new List<BehaviourInfo>();

        for (int x = 0; x < behaviours.Length; x++)
        {
            BehaviourInfo behaviourInfo = new BehaviourInfo();

            NetworkBehaviour behaviour = behaviours[x];
            Type type = behaviour.GetType();

            behaviourInfo.BehaviourType = type;
            behaviourInfo.Name = type.Name;

            FieldInfo[] fields =
                GetSyncVars(type)
                .ToArray();

            List<SyncVarInfo> syncavrs = new List<SyncVarInfo>();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                Type fieldType = field.FieldType;
                ulong mask = 1UL << i;

                syncavrs.Add(new SyncVarInfo()
                {
                    Name = field.Name,
                    Bit = mask,
                    ValueType = fieldType,
                });
            }

            behaviourInfo.SyncVars = syncavrs;

            var commands = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsDefined(typeof(CommandAttribute), inherit: true));

            foreach (var method in commands)
            {
                string fullName = BuildMirrorMethodSignature(method);
                int hash = fullName.GetStableHashCode();


            }

            var rpcs = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsDefined(typeof(ClientRpcAttribute), inherit: true));

            foreach (var method in rpcs)
            {
                string fullName = BuildMirrorMethodSignature(method);
                int hash = fullName.GetStableHashCode();
            }

            var targetRpcs = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsDefined(typeof(TargetRpcAttribute), inherit: true));

            foreach (var method in targetRpcs)
            {
                string fullName = BuildMirrorMethodSignature(method);
                int hash = fullName.GetStableHashCode();

            }

            var syncListFields = type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f =>
                    f.FieldType.IsGenericType &&
                    f.FieldType.GetGenericTypeDefinition() == typeof(SyncList<>))
                .ToArray();

            List<SyncListInfo> syncLists = new List<SyncListInfo>();

            foreach (var field in syncListFields)
            {
                Type elementType = field.FieldType.GetGenericArguments()[0];

                syncLists.Add(new SyncListInfo()
                {
                    Type = elementType
                });
            }

            behaviourInfo.SyncLists = syncLists;

            List<Type> blacklist = new List<Type>()
            {
                typeof(AspectRatioSync),
                typeof(LastHumanTracker),
                typeof(PrismaticCloud),
                typeof(TantrumEnvironmentalHazard)
            };

            if (blacklist.Contains(type))
                continue;

            behaviourInfos.Add(behaviourInfo);
        }

        foreach(var behaviourInfo in behaviourInfos)
        {
            GenerateComponentClass(path, ReplaceWhitespace(behaviourInfo.Name, string.Empty) + "Component", behaviourInfo);
        }

        string hierarchy = identity.transform.GetHierarchyPath();

        sb.AppendLine();
        sb.AppendLine($"namespace XProxy.API.Networking.Objects;");
        sb.AppendLine();
        sb.AppendLine("//");
        sb.AppendLine($"// Name: {identity.gameObject.name}");
        sb.AppendLine($"// NetworkID: {identity.netId}");
        sb.AppendLine($"// AssetID: {identity.assetId}");
        sb.AppendLine($"// SceneID: {identity.sceneId}");
        sb.AppendLine($"// Path: {hierarchy}");
        sb.AppendLine("//");
        sb.AppendLine($"public class {className} : NetworkObject");
        sb.AppendLine("{");

        sb.AppendLine("    public const uint ObjectAssetId = " + identity.assetId + ";");
        if (identity.sceneId != 0)
            sb.AppendLine("    public const ulong ObjectSceneId = " + identity.sceneId + ";");

        sb.AppendLine();

        if (identity.netId != 0)
            sb.AppendLine("    public override uint NetworkId { get; set; } = " + identity.netId + ";");

        sb.AppendLine("    public override uint AssetId { get; } = ObjectAssetId;");

        if (identity.sceneId != 0)
            sb.AppendLine("    public override ulong SceneId { get; } = ObjectSceneId;");

        for (int i = 0; i < behaviourInfos.Count; i++)
        {
            BehaviourInfo behaviourInfo = behaviourInfos[i];

            sb.AppendLine($"    public {behaviourInfo.ClassName} {behaviourInfo.NormalName} {"{ get; }"}");
        }

        sb.AppendLine();
        sb.AppendLine($"    public {className}(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)");
        sb.AppendLine("    {");
        sb.AppendLine("        //");
        sb.AppendLine($"        Behaviours = new BehaviourComponent[{behaviourInfos.Count}];");

        for(int i = 0; i < behaviourInfos.Count; i++)
        {
            BehaviourInfo behaviourInfo = behaviourInfos[i];

            sb.AppendLine();
            sb.AppendLine($"        {behaviourInfo.NormalName} = new {behaviourInfo.ClassName}(this);");
            sb.AppendLine($"        Behaviours[{i}] = {behaviourInfo.NormalName};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        File.WriteAllText(targetPath, sb.ToString());

        LabApi.Features.Console.Logger.Info("Generated object: " + className);
    }

    private void GenerateComponentClass(string path, string className, BehaviourInfo behaviourInfo)
    {
        string componentsPath = Path.Combine(path, "Components");

        string targetPath = Path.Combine(componentsPath, behaviourInfo.Name + "Component.cs");

        if (File.Exists(targetPath))
            return;

        StringBuilder sb = new StringBuilder();

        List<string> nameSpaces = new List<string>();

        for (int i = 0; i < behaviourInfo.SyncVars.Count; i++)
        {
            SyncVarInfo syncVar = behaviourInfo.SyncVars[i];

            if (!string.IsNullOrEmpty(syncVar.WriterNamespace) && !nameSpaces.Contains(syncVar.WriterNamespace))
                nameSpaces.Add(syncVar.WriterNamespace);

            if (!string.IsNullOrEmpty(syncVar.ValueType.Namespace) && !nameSpaces.Contains(syncVar.ValueType.Namespace))
                nameSpaces.Add(syncVar.ValueType.Namespace);

            if (syncVar.ValueType.IsNested && syncVar.ValueType.DeclaringType != null)
            {
                // Example: AdminToys.InvisibleInteractableToy+ColliderShape
                string staticUsing = $"static {syncVar.ValueType.DeclaringType.FullName!.Replace('+', '.')}";

                if (!nameSpaces.Contains(staticUsing))
                    nameSpaces.Add(staticUsing);
            }
        }

        foreach (var syncList in behaviourInfo.SyncLists)
        {
            if (!string.IsNullOrEmpty(syncList.Type.Namespace) && !nameSpaces.Contains(syncList.Type.Namespace))
                nameSpaces.Add(syncList.Type.Namespace);

            if (syncList.Type.IsNested && syncList.Type.DeclaringType != null)
            {
                // Example: AdminToys.InvisibleInteractableToy+ColliderShape
                string staticUsing = $"static {syncList.Type.DeclaringType.FullName!.Replace('+', '.')}";

                if (!nameSpaces.Contains(staticUsing))
                    nameSpaces.Add(staticUsing);
            }
        }

        for (int i = 0; i < nameSpaces.Count; i++)
        {
            string nameSpace = nameSpaces[i];
            sb.AppendLine($"using {nameSpace};");
        }

        sb.AppendLine();
        sb.AppendLine($"namespace XProxy.API.Networking.Components;");
        sb.AppendLine();
        sb.AppendLine($"public class {className} : BehaviourComponent");
        sb.AppendLine("{");
        sb.AppendLine();

        for (int i = 0; i < behaviourInfo.SyncVars.Count; i++)
        {
            SyncVarInfo syncVar = behaviourInfo.SyncVars[i];

            sb.AppendLine($"    private {syncVar.ValueName} {syncVar.PrivateName};");
            sb.AppendLine();
        }

        for (int i = 0; i < behaviourInfo.SyncVars.Count; i++)
        {

            SyncVarInfo syncVar = behaviourInfo.SyncVars[i];

            sb.AppendLine($"    public {syncVar.ValueName} {syncVar.NormalName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        get => {syncVar.PrivateName};");
            sb.AppendLine("        set");
            sb.AppendLine("        {");
            sb.AppendLine($"            SetSyncVarDirtyBit({syncVar.Bit});");
            sb.AppendLine($"            {syncVar.PrivateName} = value;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        componentMap.TryGetValue(behaviourInfo.BehaviourType.FullName, out ComponentGenerationInfo info);

        if (info != null)
        {
            foreach (var prop in info.ExtraProperties)
            {
                sb.AppendLine($"    public {prop.TypeName} {prop.PropertyName} {{ get; set; }}");
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        string syncListsText = string.Empty;

        if (behaviourInfo.SyncLists.Count > 0)
        {
            foreach(var syncList in behaviourInfo.SyncLists)
            {
                string typeName = syncList.Type.Name;

                if (Aliases.TryGetValue(syncList.Type, out string alias))
                    typeName = alias;

                syncListsText += $", new SyncListObject<{typeName}>()";
            }
        }

        sb.AppendLine($"    public {className}(NetworkObject networkObject) : base(networkObject{syncListsText})");
        sb.AppendLine("    {");
        sb.AppendLine("        //");
        sb.AppendLine("        this.OnSerializeSyncVars += SerializeSyncVars;");

        if (info != null)
        {
            foreach (var hook in info.SerializationHooks)
            {
                sb.AppendLine($"        this.On{hook.MethodName} += {hook.MethodName};");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    void SerializeSyncVars(NetworkWriter writer, bool forceAll)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (forceAll)");
        sb.AppendLine("        {");

        for(int x = 0; x < behaviourInfo.SyncVars.Count; x++)
        {
            SyncVarInfo syncVar = behaviourInfo.SyncVars[x];

            sb.AppendLine($"            writer.{syncVar.WriterName}({syncVar.PrivateName});");
        }

        sb.AppendLine("            return;");
        sb.AppendLine("        }");

        for (int x = 0; x < behaviourInfo.SyncVars.Count; x++)
        {
            SyncVarInfo syncVar = behaviourInfo.SyncVars[x];

            sb.AppendLine();
            sb.AppendLine($"        if ((SyncVarDirtyBits & {syncVar.Bit}U) != 0)");
            sb.AppendLine("        {");
            sb.AppendLine($"            writer.{syncVar.WriterName}({syncVar.PrivateName});");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");

        if (info != null)
        {
            foreach (var hook in info.SerializationHooks)
            {
                sb.AppendLine($"    void {hook.MethodName}(NetworkWriter writer, bool initial)");
                sb.AppendLine("    {");
                sb.AppendLine($"        if ({(hook.RunOnInitialOnly ? "" : "!")}initial)");
                sb.AppendLine("        {");

                foreach (var call in hook.WriteCalls)
                    sb.AppendLine($"            {call}");

                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        File.WriteAllText(targetPath, sb.ToString());

        LabApi.Features.Console.Logger.Info("Generated component: " + className);
    }

    public static IEnumerable<FieldInfo> GetSyncVars(Type type)
    {
        if (type == null || type == typeof(NetworkBehaviour))
            yield break;

        foreach (var field in GetSyncVars(type.BaseType))
            yield return field;

        foreach (var field in type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(f => f.IsDefined(typeof(SyncVarAttribute), true))
            .OrderBy(f => f.MetadataToken))
        {
            yield return field;
        }
    }

    // Helper: builds the same full name string Mirror uses for hashing
    private static string BuildMirrorMethodSignature(MethodInfo method)
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(method.ReturnType.FullName).Append(" ").Append(MemberFullName(method));

        MethodSignatureFullName(method, stringBuilder);

        return stringBuilder.ToString();
    }

    static string MemberFullName(MethodInfo method)
    {
        if (method.DeclaringType == null)
            return method.Name;

        return method.DeclaringType.FullName + "::" + method.Name;
    }

    static void MethodSignatureFullName(MethodInfo method, StringBuilder builder)
    {
        builder.Append("(");
        var parameters = method.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameterDefinition = parameters[i];
            if (i > 0)
            {
                builder.Append(",");
            }

            //if (parameterDefinition.ParameterType.IsSentinel)
            //{
            ///    builder.Append("...,");
            //}

            builder.Append(parameterDefinition.ParameterType.FullName.Replace("+", "/"));
        }

        builder.Append(")");
    }

    private static string FindWriterMethodName(Type fieldType, out string nameSpace)
    {
        try
        {
            Type writerGeneric = typeof(Writer<>).MakeGenericType(fieldType);

            var writeField = writerGeneric.GetField("write", BindingFlags.Public | BindingFlags.Static);

            var del = writeField?.GetValue(null) as Delegate;

            nameSpace = del?.Method?.DeclaringType?.Namespace ?? null;

            return del?.Method?.Name ?? "(no writer found)";
        }
        catch 
        { 
            nameSpace = null;
            return "(no writer found)";
        }
    }

    private static string FindReaderMethodName(Type fieldType)
    {
        try
        {
            Type readerGeneric = typeof(Reader<>).MakeGenericType(fieldType);
            var readField = readerGeneric.GetField("read", BindingFlags.Public | BindingFlags.Static);
            var del = readField?.GetValue(null) as Delegate;
            return del?.Method?.Name ?? "(no reader found)";
        }
        catch { return "(no reader found)"; }
    }

    public override void Disable()
    {
        ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
    }
}
