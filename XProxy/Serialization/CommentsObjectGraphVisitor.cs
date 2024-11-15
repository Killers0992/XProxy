using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;

namespace XProxy.Serialization
{
    public class ConsoleCommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public ConsoleCommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor) : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            ConsoleCommentsObjectDescriptor commentsObjectDescriptor = value as ConsoleCommentsObjectDescriptor;
            if (commentsObjectDescriptor != null && commentsObjectDescriptor.Comment != null)
            {
                context.Emit(new Comment(commentsObjectDescriptor.Comment, false));
            }
            return base.EnterMapping(key, value, context);
        }
    }
}
