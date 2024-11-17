using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel;

namespace XProxy.Shared.Serialization
{
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException("innerTypeDescriptor");
            }
            this._innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return from descriptor in this._innerTypeDescriptor.GetProperties(type, container)
                   select new CommentsPropertyDescriptor(descriptor);
        }

        public override string GetEnumName(Type enumType, string name)
        {
            return this._innerTypeDescriptor.GetEnumName(enumType, name);
        }

        public override string GetEnumValue(object enumValue)
        {
            return this._innerTypeDescriptor.GetEnumValue(enumValue);
        }

        private readonly ITypeInspector _innerTypeDescriptor;
    }
}
