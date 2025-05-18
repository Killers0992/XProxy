namespace XProxy.Models;

public struct PublicKey
{
    public PublicKey(string key, string signature, string credits)
    {
        this.Key = key;
        this.Signature = signature;
        this.Credits = credits;
    }

    public string Key;
    public string Signature;
    public string Credits;
}
