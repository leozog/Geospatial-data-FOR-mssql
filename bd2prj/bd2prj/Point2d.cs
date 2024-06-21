using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.IO;
using System.Globalization;
using Microsoft.SqlServer.Server;

namespace bd2prj
{
    /// <summary>
    /// Represents a 2D point with x and y coordinates.
    /// </summary>
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(
        Format.UserDefined,
        IsByteOrdered = true,
        MaxByteSize = 20
    )]
    public class Point2d : INullable, IBinarySerialize
    {
        private double x;
        private double y;
        private Geohash geohash;
        private bool _null;

        /// <summary>
        /// Returns true if the point is Null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return _null;
            }
        }

        /// <summary>
        /// Gets a Null Point2d object.
        /// </summary>
        public static Point2d Null
        {
            get
            {
                Point2d h = new Point2d();
                h._null = true;
                return h;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Point2d class with the specified x and y coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        public Point2d(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.geohash = Geohash.Null;
            this._null = false;
        }

        /// <summary>
        /// Initializes a new instance of the Point2d class with default coordinates (0, 0).
        /// </summary>
        public Point2d() : this(0, 0)
        {
        }

        /// <summary>
        /// Returns a string representation of the Point2d object.
        /// </summary>
        /// <returns>A string representation of the Point2d object.</returns>
        public override string ToString()
        {
            if (IsNull)
                return "NULL";
            StringBuilder sb = new StringBuilder();
            sb.Append(x.ToString(CultureInfo.InvariantCulture));
            sb.Append(",");
            sb.Append(y.ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        /// <summary>
        /// Parses a string representation of a Point2d object and returns a new instance of the Point2d class.
        /// </summary>
        /// <param name="S">The string representation of the Point2d object, in form x,y where x and y are float values with '.' decimator.</param>
        /// <returns>A new instance of the Point2d class.</returns>
        public static Point2d Parse(SqlString S)
        {
            if (S.IsNull)
                return Null;
            string[] parts = S.Value.Split(',');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Error (1) parsing point string " + S.Value);
            }
            if (!double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double x))
            {
                throw new FormatException("Error (2) parsing point string " + S.Value);
            }
            if (!double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                throw new FormatException("Error (3) parsing point string " + S.Value);
            }
            return new Point2d(x, y);
        }

        /// <summary>
        /// Compares the current Point2d object with another Point2d, based on their Geohash representation.
        /// </summary>
        /// <param name="obj">The object to compare with the current Point2d object.</param>
        /// <returns>A value that indicates the order of the objects being compared.</returns>
        public int CompareTo(object obj)
        {
            if (obj is Point2d)
            {
                Point2d p = (Point2d)obj;
                return Geohash.CompareTo(p.Geohash);
            }
            else
            {
                throw new ArgumentException("Cant compare Point2d to " + obj.GetType().ToString());
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the point.
        /// </summary>
        public double X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the point.
        /// </summary>
        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        /// <summary>
        /// Gets the Geohash representation of the point.
        /// </summary>
        public Geohash Geohash
        {
            get
            {
                if (geohash.IsNull)
                    geohash = new Geohash(x, y);
                return geohash;
            }
        }

        /// <summary>
        /// Determines whether the point is visible from the rectangle window specified by two points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns>True if the point is visible.</returns>
        public bool VisibleFrom(double x1, double y1, double x2, double y2)
        {
            if (IsNull)
                return false;
            double xmin = Math.Min(x1, x2);
            double xmax = Math.Max(x1, x2);
            double ymin = Math.Min(y1, y2);
            double ymax = Math.Max(y1, y2);
            if (X > xmax || X < xmin || Y > ymax || Y < ymin)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Calculates the Euclidean distance between the current point and another point.
        /// </summary>
        /// <param name="other">The other point to calculate the distance to.</param>
        /// <returns>The Euclidean distance between the two points.</returns>
        public double Dist(Point2d other)
        {
            if (IsNull || other.IsNull)
                return double.NaN;
            return Math.Sqrt((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y));
        }

        /// <summary>
        /// Sets the x and y coordinates of the point.
        /// </summary>
        /// <param name="x">The new x-coordinate of the point.</param>
        /// <param name="y">The new y-coordinate of the point.</param>
        public void SetXY(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.geohash = Geohash.Null;
        }

        /// <summary>
        /// Writes the binary representation of the Point2d object to a BinaryWriter.
        /// </summary>
        /// <param name="w">The BinaryWriter to write to.</param>
        public void Write(BinaryWriter w)
        {
            w.Write(x);
            w.Write(y);
            geohash.Write(w);
        }

        /// <summary>
        /// Reads the binary representation of the Point2d object from a BinaryReader.
        /// </summary>
        /// <param name="r">The BinaryReader to read from.</param>
        public void Read(BinaryReader r)
        {
            x = r.ReadDouble();
            y = r.ReadDouble();
            geohash.Read(r);
        }
    }
}