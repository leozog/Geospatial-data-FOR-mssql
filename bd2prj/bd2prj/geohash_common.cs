using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.IO;

namespace bd2prj
{
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedAggregate(Format.UserDefined, MaxByteSize = -1)]
    public struct geohash_common : IBinarySerialize
    {
        public void Init()
        {
            first = true;
        }

        public void Accumulate(SqlString geohash)
        {
            if (geohash.IsNull)
                return;
            if (first)
            {
                first = false;
                common = geohash.Value;
            }
            else
            {
                int i = 0;
                while (i < common.Length && i < geohash.Value.Length && common[i] == geohash.Value[i])
                    i++;
                common = common.Substring(0, i);
            }
        }

        public void Merge(geohash_common other)
        {
            Accumulate(other.common);
        }

        public SqlString Terminate()
        {
            return new SqlString(common);
        }

        // This is a place-holder member field
        public bool first;
        public string common;

        public void Read(BinaryReader r)
        {
            first = r.ReadBoolean();
            common = r.ReadString();
        }
        public void Write(BinaryWriter w)
        {
            w.Write(first);
            w.Write(common);
        }
    }
}