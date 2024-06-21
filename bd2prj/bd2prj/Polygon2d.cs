using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.SqlServer.Server;

namespace bd2prj
{
    /// <summary>
    /// Represents a 2D polygon.
    /// </summary>
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(
        Format.UserDefined,
        IsByteOrdered = true,
        MaxByteSize = -1
    )]
    public class Polygon2d : INullable, IBinarySerialize
    {
        private List<Point2d> points;
        private Point2d borderMin;
        private Point2d borderMax;
        private double area;
        private bool _null;

        /// <summary>
        /// Returns true if the polygon is Null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return _null;
            }
        }

        /// <summary>
        /// Gets a Null Polygon2d object.
        /// </summary>
        public static Polygon2d Null
        {
            get
            {
                Polygon2d h = new Polygon2d();
                h._null = true;
                return h;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Polygon2d class with the specified points.
        /// </summary>
        /// <param name="points">The points that define the polygon.</param>
        public Polygon2d(List<Point2d> points)
        {
            this.points = points;
            this.borderMin = Point2d.Null;
            this.borderMax = Point2d.Null;
            this.area = double.NaN;
            this._null = false;
        }

        /// <summary>
        /// Initializes a new instance of the Polygon2d class.
        /// </summary>
        public Polygon2d() : this(new List<Point2d>())
        {
        }

        /// <summary>
        /// Invalidates the calculated fields of the polygon.
        /// </summary>
        private void invalidateCalcFields()
        {
            borderMin = Point2d.Null;
            borderMax = Point2d.Null;
            area = double.NaN;
        }

        /// <summary>
        /// Returns a string representation of the polygon.
        /// </summary>
        /// <returns>A string representation of the polygon.</returns>
        public override string ToString()
        {
            if (IsNull)
                return "NULL";
            StringBuilder sb = new StringBuilder();
            foreach (Point2d p in points)
            {
                sb.Append(p.ToString());
                sb.Append(";");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses a string and returns a Polygon2d object.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A Polygon2d object.</returns>
        public static Polygon2d Parse(SqlString s)
        {
            if (s.IsNull)
                return Null;
            string[] parts = s.Value.Split(';');
            List<Point2d> points = new List<Point2d>();
            foreach (string part in parts)
            {
                if (part.Trim().Length == 0)
                    continue;
                points.Add(Point2d.Parse(part));
            }
            return new Polygon2d(points);
        }

        /// <summary>
        /// Gets the minimum border point of the polygon.
        /// </summary>
        public Point2d BorderMin
        {
            get
            {
                if (borderMin.IsNull)
                {
                    double xmin = points.Min(p => p.X);
                    double ymin = points.Min(p => p.Y);
                    borderMin = new Point2d(xmin, ymin);
                }
                return borderMin;
            }
        }

        /// <summary>
        /// Gets the maximum border point of the polygon.
        /// </summary>
        public Point2d BorderMax
        {
            get
            {
                if (borderMax.IsNull)
                {
                    double xmax = points.Max(p => p.X);
                    double ymax = points.Max(p => p.Y);
                    borderMax = new Point2d(xmax, ymax);
                }
                return borderMax;
            }
        }

        /// <summary>
        /// Gets the minimum geohash of the polygon.
        /// </summary>
        public Geohash GeohashMin
        {
            get
            {
                return BorderMin.Geohash;
            }
        }

        /// <summary>
        /// Gets the maximum geohash of the polygon.
        /// </summary>
        public Geohash GeohashMax
        {
            get
            {
                return BorderMax.Geohash;
            }
        }

        /// <summary>
        /// Determines whether the polygon is visible from the rectangle window specified by two points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns>True if the polygon is visible, false otherwise.</returns>
        public bool VisibleFrom(double x1, double y1, double x2, double y2)
        {
            if (IsNull)
                return false;
            double xmin = Math.Min(x1, x2);
            double xmax = Math.Max(x1, x2);
            double ymin = Math.Min(y1, y2);
            double ymax = Math.Max(y1, y2);
            if (BorderMin.X > xmax || BorderMax.X < xmin || BorderMin.Y > ymax || BorderMax.Y < ymin)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Gets the center point of the polygon.
        /// </summary>
        public Point2d Center
        {
            get
            {
                return new Point2d((BorderMin.X + BorderMax.X) / 2, (BorderMin.Y + BorderMax.Y) / 2);
            }
        }

        /// <summary>
        /// Gets the area of the polygon.
        /// </summary>
        public double Area
        {
            get
            {
                if (Double.IsNaN(area))
                {
                    double sum = 0;
                    for (int i = 0; i < points.Count; i++)
                    {
                        Point2d p1 = points[i];
                        Point2d p2 = points[(i + 1) % points.Count];
                        sum += p1.X * p2.Y - p2.X * p1.Y;
                    }
                    area = Math.Abs(sum) / 2;
                }
                return area;
            }
        }

        /// <summary>
        /// Determines whether the polygon includes the specified point.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the polygon includes the point.</returns>
        public bool Includes(Point2d point)
        {
            if (IsNull)
                return false;
            if (point.Geohash.CompareTo(GeohashMin) < 0 || point.Geohash.CompareTo(GeohashMax) > 0)
                return false;
            return IncludesNoGeohashCheck(point);
        }

        /// <summary>
        /// Determines whether the polygon includes the specified point without geohash check.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the polygon includes the point.</returns>
        public bool IncludesNoGeohashCheck(Point2d point)
        {
            if (IsNull)
                return false;
            bool result = false;
            for (int i = 0; i < points.Count; i++)
            {
                Point2d p1 = points[i];
                Point2d p2 = points[(i + 1) % points.Count];
                if (p1.Y < point.Y && p2.Y >= point.Y || p2.Y < point.Y && p1.Y >= point.Y)
                {
                    if (p1.X + (point.Y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X) < point.X)
                        result = !result;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds a point to the polygon.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Point2d point)
        {
            points.Add(point);
            invalidateCalcFields();
        }

        /// <summary>
        /// Removes the last point from the polygon.
        /// </summary>
        /// <param name="point">The point to remove.</param>
        public void RemovePoint(Point2d point)
        {
            points.Remove(point);
            invalidateCalcFields();
        }

        /// <summary>
        /// Gets or sets the point at the specified index.
        /// </summary>
        /// <param name="index">The index of the point.</param>
        /// <returns>The point at the specified index.</returns>
        public Point2d this[int index]
        {
            get
            {
                return points[index];
            }
            set
            {
                points[index] = value;
                invalidateCalcFields();
            }
        }

        /// <summary>
        /// Reads the Geohash from the specified BinaryReader.
        /// </summary>
        /// <param name="r">The BinaryReader to read from.</param>
        public void Write(System.IO.BinaryWriter w)
        {
            borderMin.Write(w);
            borderMax.Write(w);
            w.Write(area);
            w.Write(points.Count);
            foreach (Point2d p in points)
            {
                p.Write(w);
            }
        }

        /// <summary>
        /// Writes the Geohash to the specified BinaryWriter.
        /// </summary>
        /// <param name="w">The BinaryWriter to write to.</param>
        public void Read(System.IO.BinaryReader r)
        {
            borderMin.Read(r);
            borderMax.Read(r);
            area = r.ReadDouble();
            int count = r.ReadInt32();
            points = new List<Point2d>(count);
            for (int i = 0; i < count; i++)
            {
                Point2d p = new Point2d();
                p.Read(r);
                points.Add(p);
            }
        }
    }
}