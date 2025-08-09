using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Models;

public class SyncObjectInfo
{
    public void OnSerializeAll(NetworkWriter writer)
    {
        Logger.Info("Serialize all SYNCOBJECT");
        writer.WriteUInt(0);
        writer.WriteUInt(0);
    }

    public void OnSerializeDelta(NetworkWriter writer)
    {
        writer.WriteUInt(0);
    }
}
