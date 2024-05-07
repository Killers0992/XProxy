using LiteNetLib.Utils;
using System;
using System.Text;
using XProxy.Enums;

namespace XProxy.Models
{
    public class PreAuthModel
    {
        public NetDataWriter RawPreAuth;

        public static PreAuthModel ReadPreAuth(string endpoint, NetDataReader reader, ref string failedOn)
        {
            PreAuthModel model = new PreAuthModel();
            model.IpAddress = endpoint;

            model.RawPreAuth = NetDataWriter.FromBytes(reader.RawData, reader.UserDataOffset, reader.UserDataSize);

            failedOn = "Client Type";
            if (!reader.TryGetByte(out byte clientType)) return model;
            model.ClientType = (ClientType)clientType;

            failedOn = "Major Version";
            if (!reader.TryGetByte(out byte major)) return model;
            model.Major = major;

            failedOn = "Minor Version";
            if (!reader.TryGetByte(out byte minor)) return model;
            model.Minor = minor;

            failedOn = "Revision Version";
            if (!reader.TryGetByte(out byte revision)) return model;
            model.Revision = revision;

            failedOn = "Backward Compatibility";
            if (!reader.TryGetBool(out bool backwardCompatibility)) return model;
            model.BackwardCompatibility = backwardCompatibility;

            if (backwardCompatibility)
            {
                failedOn = "Backward Revision";
                if (!reader.TryGetByte(out byte backwardRevision)) return model;
                model.BackwardRevision = backwardRevision;
            }

            failedOn = "ChallengeID";
            if (!reader.TryGetInt(out int challengeid)) return model;
            model.ChallengeID = challengeid;

            if (model.ChallengeID != 0)
            {
                failedOn = "ChallengeResponse";
                if (!reader.TryGetBytesWithLength(out byte[] challenge)) return model;
                model.ChallengeResponse = challenge;
            }
            else
                model.ChallengeResponse = new byte[0];

            failedOn = "UserID";
            if (!reader.TryGetString(out string userid)) return model;

            failedOn = "UserID is null/empty ( player not authenticated )";
            if (string.IsNullOrEmpty(userid)) return model;
            model.UserID = userid;

            failedOn = "Expiration";
            if (!reader.TryGetLong(out long expiration)) return model;
            model.Expiration = expiration;

            failedOn = "Flags";
            if (!reader.TryGetByte(out byte flags)) return model;
            model.Flags = (CentralAuthPreauthFlags)flags;

            failedOn = "Region";
            if (!reader.TryGetString(out string region)) return model;
            model.Region = region;

            failedOn = "Signature";
            if (!reader.TryGetBytesWithLength(out byte[] signature)) return model;
            model.Signature = signature;

            model.IsValid = true;
            return model;
        }

        public bool IsValid { get; set; }
        public string IpAddress { get; set; }
        public ClientType ClientType { get; set; }
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }
        public bool BackwardCompatibility { get; set; }
        public byte BackwardRevision { get; set; }

        public int ChallengeID { get; set; }
        public byte[] ChallengeResponse { get; set; }

        public string UserID { get; set; } = "Unknown UserID";

        public long Expiration { get; set; }

        public CentralAuthPreauthFlags Flags { get; set; }

        public string Region { get; set; } = "Unknown Region";

        public byte[] Signature { get; set; } = new byte[0];

        public override string ToString()
        {
            return string.Concat(
                $"Client Type: {ClientType}",
                Environment.NewLine,
                $"Version: {Major}.{Minor}.{Revision}, Backward Compatibility: {(BackwardCompatibility ? "NO" : $"YES ( Revision {BackwardRevision} )")}",
                Environment.NewLine,
                $"Challenge ID: {ChallengeID}",
                Environment.NewLine,
                $"Challenge: {Encoding.UTF8.GetString(ChallengeResponse)}",
                Environment.NewLine,
                $"UserID: {UserID}",
                Environment.NewLine,
                $"Expiration: {Expiration}",
                Environment.NewLine,
                $"Flags: {Flags}",
                Environment.NewLine,
                $"Region: {Region}",
                Environment.NewLine,
                $"Signature length: {Signature.Length}");
        }
    }
}
