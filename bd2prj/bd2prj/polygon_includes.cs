using System;  
using System.Data.Sql;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;  
using System.Collections;  
using System.Data.SqlTypes;  
using System.Diagnostics;  
using System.Collections.Generic;
using System.Globalization;

namespace bd2prj
{
    public partial class UserDefinedFunctions
    {
        /// <summary>
        /// Retrieves a collection IDs of polygons containing a specified point, utilizing the optimization of quering by indexed geohash fields.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="min_area">The minimum area required for the polygons.</param>
        /// <returns>A collection of polygon IDs containing the specified point.</returns>
        [SqlFunction(FillRowMethodName = "polygon_includes_FillRow", TableDefinition = "polygon_id int", DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static IEnumerable polygon_includes(SqlDouble x, SqlDouble  y, SqlDouble min_area)
        {
            List<int> polygons = new List<int>();
            using (SqlConnection connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT polygon_id FROM dbo.polygon 
                                            WHERE area >= @min_area
                                            AND geohashMin <= CONVERT(dbo.Point2d, @point).Geohash 
                                            AND geohashMax >= CONVERT(dbo.Point2d, @point).Geohash
                                            AND p.IncludesNoGeohashCheck(CONVERT(dbo.Point2d, @point)) = 1;";
                    command.Parameters.AddWithValue("@min_area", min_area);
                    command.Parameters.AddWithValue("@point", x.Value.ToString(CultureInfo.InvariantCulture) + "," + y.Value.ToString(CultureInfo.InvariantCulture));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            polygons.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return polygons;
        }

        public static void polygon_includes_FillRow(object obj, out SqlInt32 polygon_id)
        {
            polygon_id = (int)obj;
        }
    }
}