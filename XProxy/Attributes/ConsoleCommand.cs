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
