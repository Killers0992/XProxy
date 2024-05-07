using System;
using System.ComponentModel;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace XProxy.Serialization
{
    internal sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this._baseDescriptor = baseDescriptor;
            this.Name = baseDescriptor.Name;
        }

        public string Name { get; set; }

        public Type Type
        {
            get
            {
                return this._baseDescriptor.Type;
            }
        }

        public Type TypeOverride
        {
            get
            {
                return this._baseDescriptor.TypeOverride;
            }
            set
            {
                this._baseDescriptor.TypeOverride = value;
            }
        }

        public int Order { get; set; }

        public ScalarStyle ScalarStyle
        {
            get
            {
                return this._baseDescriptor.ScalarStyle;
            }
            set
            {
                this._baseDescriptor.ScalarStyle = value;
            }
        }

        public bool CanWrite
        {
            get
            {
                return this._baseDescriptor.CanWrite;
            }
        }

        public void Write(object target, object value)
        {
            this._baseDescriptor.Write(target, value);
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return this._baseDescriptor.GetCustomAttribute<T>();
        }

        public IObjectDescriptor Read(object target)
        {
            DescriptionAttribute customAttribute = this._baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            if (customAttribute == null)
            {
                return this._baseDescriptor.Read(target);
            }
            return new CommentsObjectDescriptor(this._baseDescriptor.Read(target), customAttribute.Description);
        }

        private readonly IPropertyDescriptor _baseDescriptor;
    }
}
