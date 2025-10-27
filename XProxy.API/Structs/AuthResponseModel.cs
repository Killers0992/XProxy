namespace XProxy.API.Structs;

public struct AuthResponseModel
{
    public bool Success { get; set; }
    public bool Verified { get; set; }
    public string Error { get; set; }
    public string Token { get; set; }
    public string[] Messages { get; set; }
    public string[] Actions { get; set; }
    public string[] AuthAccepted { get; set; }
    public AuthRejectModel[] AuthRejected { get; set; }
    public string VerificationChallenge { get; set; }
    public string VerificationResponse { get; set; }
}
