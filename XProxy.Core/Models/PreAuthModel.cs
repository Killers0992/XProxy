using CentralAuth;
using LiteNetLib.Utils;
using System;
using System.Text;
using XProxy.Core;
using XProxy.Cryptography;
using XProxy.Enums;
using XProxy.Services;

namespace XProxy.Models
{
    public class PreAuthModel
    {
        public NetDataWriter RawPreAuth;
        public Server Server;

        public static PreAuthModel ReadPreAuth(string endpoint, NetDataReader reader, ref string failedOn)
        {
            PreAuthModel model = new PreAuthModel();
            model.IpAddress = endpoint;

            model.RawPreAuth = NetDataWriter.FromBytes(reader.RawData, reader.UserDataOffset, reader.UserDataSize);

            failedOn = "Client Type";

            if (!reader.TryGetByte(out byte clientType)) 
                return model;

            model.ClientType = (ClientType)clientType;

            if (model.ClientType == ClientType.Proxy)
            {
                if (!reader.TryGetString(out string connectionKey))
                    return model;

                if (!reader.TryGetUShort(out ushort port))
                    return model;

                if (!Server.TryGetByIp(endpoint, port, out Server server))
                    return model;

                if (connectionKey != server.Settings.PluginExtension.ConnectionKey)
                    return model;

                // If allowed connections contains any defined ip check if incoming endpoint is in that list.
                if (server.Settings.PluginExtension.AllowedConnections.Length > 0)
                {
                    if (!server.Settings.PluginExtension.AllowedConnections.Contains(endpoint))
                        return null;
                }

                model.Server = server;
                return model;
            }

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
            
            failedOn = "Expiration check";
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiration) return model;

            failedOn = "Flags";
            if (!reader.TryGetByte(out byte flags)) return model;
            model.Flags = (CentralAuthPreauthFlags)flags;

            failedOn = "Region";
            if (!reader.TryGetString(out string region)) return model;
            model.Country = region;

            failedOn = "Signature";
            if (!reader.TryGetBytesWithLength(out byte[] signature)) return model;
            model.Signature = signature;

            failedOn = "Signature check";
            if (!ECDSA.VerifyBytes($"{userid};{flags};{region};{expiration}", signature, PublicKeyService.PublicKey))
                return model;

            model.IsValid = true;
            return model;
        }

        public NetDataWriter CreateChallenge(int challengeId, byte[] challengeResponse)
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((byte)ClientType.GameClient);
            writer.Put(Major);
            writer.Put(Minor);
            writer.Put(Revision);
            writer.Put(BackwardCompatibility);

            if (BackwardCompatibility)
                writer.Put(BackwardRevision);

            writer.Put(challengeId);
            writer.PutBytesWithLength(challengeResponse);

            writer.Put(UserID);
            writer.Put(Expiration);
            writer.Put((byte)Flags);
            writer.Put(Country);
            writer.PutBytesWithLength(Signature);

            writer.Put(IpAddress);

            return writer;
        }

        public bool IsValid { get; set; }
        public string IpAddress { get; set; }
        public ClientType ClientType { get; set; }
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }
        public string Version => $"{Major}.{Minor}.{Revision}";
        public bool BackwardCompatibility { get; set; }
        public byte BackwardRevision { get; set; }

        public int ChallengeID { get; set; }
        public byte[] ChallengeResponse { get; set; }

        public string UserID { get; set; } = "Unknown UserID";

        public long Expiration { get; set; }

        public CentralAuthPreauthFlags Flags { get; set; }

        public string Country { get; set; } = "Unknown Country";

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
                $"Region: {Country}",
                Environment.NewLine,
                $"Signature length: {Signature.Length}");
        }
    }
}
