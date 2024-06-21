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
        /// Retrieves a collection IDs of polygons within a specified window, utilizing the optimization of quering by indexed geohash fields.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point defining the window.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <param name="min_area">The minimum area of the polygons to be retrieved.</param>
        /// <returns>A collection of polygon IDs within the specified window.</returns>
        [SqlFunction(FillRowMethodName = "polygons_in_window_FillRow", TableDefinition = "polygon_id int", DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static IEnumerable polygons_in_window(SqlDouble x1, SqlDouble  y1, SqlDouble x2, SqlDouble y2, SqlDouble min_area)
        {
            double xmin = Math.Max(-180, Math.Min(x1.Value, x2.Value));
            double xmax = Math.Min(180, Math.Max(x1.Value, x2.Value));
            double ymin = Math.Max(-90, Math.Min(y1.Value, y2.Value));
            double ymax = Math.Min(90, Math.Max(y1.Value, y2.Value));
            List<int> polygons = new List<int>();
            using (SqlConnection connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT polygon_id FROM dbo.polygon 
                                            WHERE area >= @min_area
                                            AND geohashMin <= CONVERT(dbo.Point2d, @max).Geohash 
                                            AND geohashMax >= CONVERT(dbo.Point2d, @min).Geohash 
                                            AND p.VisibleFrom(@xmin, @ymin, @xmax, @ymax) = 1";
                    command.Parameters.AddWithValue("@min_area", min_area);
                    command.Parameters.AddWithValue("@min", xmin.ToString(CultureInfo.InvariantCulture) + "," + ymin.ToString(CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@max", xmax.ToString(CultureInfo.InvariantCulture) + "," + ymax.ToString(CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@xmin", xmin);
                    command.Parameters.AddWithValue("@ymin", ymin);
                    command.Parameters.AddWithValue("@xmax", xmax);
                    command.Parameters.AddWithValue("@ymax", ymax);
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

        public static void polygons_in_window_FillRow(object obj, out SqlInt32 polygon_id)
        {
            polygon_id = (int)obj;
        }
    }
}