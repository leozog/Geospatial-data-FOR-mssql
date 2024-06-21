using System;  
using System.Data.Sql;  
using Microsoft.SqlServer.Server;  
using System.Collections;  
using System.Data.SqlTypes;  
using System.Diagnostics;  
using System.Collections.Generic;

namespace bd2prj
{
    public partial class UserDefinedFunctions
    {
        [SqlFunction(FillRowMethodName = "geohash_array_FillRow", TableDefinition = "a Geohash")]
        public static IEnumerable geohash_array(SqlDouble x1, SqlDouble y1, SqlDouble x2, SqlDouble y2, SqlInt32 precision)
        {
            int p = precision.IsNull ? 12 : precision.Value;
            double minlat = Math.Min(y1.Value, y2.Value);
            double maxlat = Math.Max(y1.Value, y2.Value);
            double minlon = Math.Min(x1.Value, x2.Value);
            double maxlon = Math.Max(x1.Value, x2.Value);

            double latstep = Geohash.size_y(p);
            double lonstep = Geohash.size_x(p);

            var geohashes = new List<Geohash>();
            for (double lat = minlat; lat < maxlat + latstep; lat += latstep)
            {
                for (double lon = minlon; lon < maxlon + latstep; lon += lonstep)
                    geohashes.Add(new Geohash(lon, lat, p));
            }
            return geohashes;
        }

        public static void geohash_array_FillRow(object obj, out Geohash a)
        {
            a = (Geohash)obj;
        }
    }
}