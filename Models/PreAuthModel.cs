using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy.Models
{
    public class PreAuthModel
    {
        public NetDataWriter RawPreAuth;

        public NetDataWriter RegenPreAuth()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(b);
            writer.Put(Major);
            writer.Put(Minor);
            writer.Put(Revision);
            writer.Put(BackwardRevision);
            writer.Put(flag);
            writer.Put(0);
            writer.Put(UserID);
            writer.Put(Expiration);
            writer.Put((byte)Flags);
            writer.Put(Region);
            writer.PutBytesWithLength(Signature);
            return writer;
        }

        public static PreAuthModel ReadPreAuth(string endpoint, NetDataReader reader)
        {

            PreAuthModel model = new PreAuthModel();
            model.IpAddress = endpoint;

            model.RawPreAuth = NetDataWriter.FromBytes(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
            model.RawPreAuth.Put(endpoint);

            if (reader.TryGetByte(out byte b))
            {
                model.b = b;
           //     Console.WriteLine("Receive B " + b );
            }

            byte cBackwardRevision = 0;
            byte cMajor;
            byte cMinor;
            byte cRevision;
            bool cflag;
            if (!reader.TryGetByte(out cMajor) || !reader.TryGetByte(out cMinor) || !reader.TryGetByte(out cRevision) || !reader.TryGetBool(out cflag) || (cflag && !reader.TryGetByte(out cBackwardRevision)))
            {
                return null;
            }

            model.Major = cMajor;
            model.Minor = cMinor;
            model.Revision = cRevision;
            model.BackwardRevision = cBackwardRevision;
            model.flag = cflag;

            if (reader.TryGetInt(out int challengeid))
            {
                model.ChallengeID = challengeid;
               // Console.WriteLine("Receive challenge ID" + challengeid);
            }

            if (reader.TryGetBytesWithLength(out byte[] challenge))
            {
                model.Challenge = challenge;
               // Console.WriteLine("Receive challenge " + Encoding.UTF8.GetString(challenge));
            }

            if (reader.TryGetString(out string userid))
            {
                model.UserID = userid;
               // Console.WriteLine("Receive userid " + userid );
            }  

            if (reader.TryGetLong(out long expiration))
            {
                model.Expiration = expiration;
               // Console.WriteLine("Receive expiration " + expiration);
            }

            if (reader.TryGetByte(out byte flags))
            {
                model.Flags = (CentralAuthPreauthFlags)flags;
                //Console.WriteLine("Receive flags");
            }

            if (reader.TryGetString(out string region))
            {
                model.Region = region;
               // Console.WriteLine("Receive region");
            }

            if (reader.TryGetBytesWithLength(out byte[] signature))
            {
                model.Signature = signature;
               // Console.WriteLine("Receive signature");
            }

            return model;
        }

        public string IpAddress { get; set; }


        public byte b { get; set; }

        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }
        public byte BackwardRevision { get; set; }

        public bool flag { get; set; }

        public int ChallengeID { get; set; }
        public byte[] Challenge { get; set; }

        public string UserID { get; set; } = "Unknown UserID";

        public long Expiration { get; set; }

        public CentralAuthPreauthFlags Flags { get; set; }

        public string Region { get; set; } = "Unknown Region";

        public byte[] Signature { get; set; } = new byte[0];

        public override string ToString()
        {
            return string.Concat(
                $"Version: {Major}.{Minor}.{Revision}, Backward revision: {BackwardRevision}",
                Environment.NewLine,
                $"Challenge ID: {ChallengeID}",
                Environment.NewLine,
                $"Challenge: {Encoding.UTF8.GetString(Challenge)}",
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
