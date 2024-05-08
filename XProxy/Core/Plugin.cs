using System;

namespace XProxy.Core
{
    public class Plugin
    {
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual string Author { get; }
        public virtual Version Version { get; }

        public virtual void OnLoad()
        {

        }

        public virtual void OnUnload()
        {

        }
    }
}
