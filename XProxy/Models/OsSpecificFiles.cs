namespace XProxy.Models
{
    public class OsSpecificFiles
    {
        public BuildFile Proxy { get; set; } = new BuildFile();
        public BuildFile Dependencies { get; set; } = new BuildFile();
    }
}
