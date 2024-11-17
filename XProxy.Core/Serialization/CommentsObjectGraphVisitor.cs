using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;

namespace XProxy.Shared.Serialization
{
    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor) : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
        {
            CommentsObjectDescriptor commentsObjectDescriptor = value as CommentsObjectDescriptor;
            if (commentsObjectDescriptor != null && commentsObjectDescriptor.Comment != null)
            {
                context.Emit(new Comment(commentsObjectDescriptor.Comment, false));
            }

            return base.EnterMapping(key, value, context, serializer);
        }
    }
}
