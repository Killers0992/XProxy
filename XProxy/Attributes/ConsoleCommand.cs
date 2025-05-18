using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommand : Attribute
    {
        public string Name { get; set; }

        public ConsoleCommand(string name)
        {
            Name = name;
        }
    }
}
