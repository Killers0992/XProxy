namespace XProxy.API.Structs;

public struct PublicKeyResponseModel
{
    public string Key { get; set; }
    public string Signature { get; set; }
    public string Credits { get; set; }
}
