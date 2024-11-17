using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using YamlDotNet.Core;

namespace XProxy.Shared.Serialization
{
    public static class YamlParser
    {
        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithEmissionPhaseObjectGraphVisitor<CommentsObjectGraphVisitor>((EmissionPhaseObjectGraphVisitorArgs visitor) =>
            new CommentsObjectGraphVisitor(visitor.InnerVisitor))
                .WithTypeInspector<CommentGatheringTypeInspector>((ITypeInspector typeInspector) =>
                new CommentGatheringTypeInspector(typeInspector))
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .DisableAliases()
                    .IgnoreFields()
                    .WithDefaultScalarStyle(ScalarStyle.SingleQuoted)
                    .Build();

        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .IgnoreFields()
            .Build();
    }
}
