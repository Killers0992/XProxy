namespace XProxy.Core;

public class Server
{
    public string IpAddress { get; set; }
    public int Port { get; set; }

    public bool IncludeIpInPreauth { get; private set; }

    public Server(string ip, int port)
    {
        IpAddress = ip; 
        Port = port;
    }
}
