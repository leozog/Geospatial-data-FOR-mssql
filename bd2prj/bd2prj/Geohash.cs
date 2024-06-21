using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Server;

namespace bd2prj
{
    /// <summary>
    /// Represents a Geohash, which is a string representation of a geographic location.
    /// </summary>
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedType(
        Format.UserDefined,
        IsByteOrdered = true,
        MaxByteSize = 16
        )]
    public class Geohash: INullable, IBinarySerialize
    {
        const string base32 = "0123456789bcdefghjkmnpqrstuvwxyz";
        private string value;
        private bool _null;
        
        /// <summary>
        /// Returns true if the Geohash is Null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return _null;
            }
        }
        
        /// <summary>
        /// Gets a Null Geohash object.
        /// </summary>
        public static Geohash Null
        {
            get
            {
                Geohash h = new Geohash();
                h._null = true;
                return h;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Geohash class with the specified string value.
        /// </summary>
        /// <param name="s">The string value of the Geohash.</param>
        public Geohash(string s)
        {
            this.value = s;
            this._null = false;
        }

        /// <summary>
        /// Initializes a new instance of the Geohash class representing the whoe world.
        /// </summary>
        public Geohash() : this("")
        {
        }
    
        /// <summary>
        /// Initializes a new instance of the Geohash class with the specified coordinates and precision.
        /// </summary>
        /// <param name="x">The longitude coordinate.</param>
        /// <param name="y">The latitude coordinate.</param>
        /// <param name="precision">The precision of the Geohash.</param>
        public Geohash(double x, double y, int precision = 12)
        {
            if (x < -180 || x > 180 || y < -90 || y > 90 || precision < 0 || precision > 12)
            {
                this.value = "";
                this._null = false;
                return;
            }

            StringBuilder s = new StringBuilder(12);

            double[] lon = new double[2] { -180, 180 };
            double[] lat = new double[2] { -90, 90 };

            int ch = 0;
            for (int i = 0; i < precision * 5; i++)
            {
                ch <<= 1;
                if (i % 2 == 0)
                {
                    double mid = (lon[0] + lon[1]) / 2;
                    if (x > mid)
                    {
                        ch |= 1;
                        lon[0] = mid;
                    }
                    else
                        lon[1] = mid;
                }
                else
                {
                    double mid = (lat[0] + lat[1]) / 2;
                    if (y > mid)
                    {
                        ch |= 1;
                        lat[0] = mid;
                    }
                    else
                        lat[1] = mid;
                }
                if (i % 5 == 4)
                {
                    s.Append(base32[ch]);
                    ch = 0;
                }
            }
            this.value = s.ToString();
            this._null = false;
        }
        
        /// <summary>
        /// Returns a string that represents the current Geohash.
        /// </summary>
        /// <returns>A string that represents the current Geohash.</returns>
        public override string ToString()
        {
            if (IsNull)
                return "NULL";
            return value;
        }

        /// <summary>
        /// Parses the specified SqlString and returns a Geohash object.
        /// </summary>
        /// <param name="s">The SqlString to parse.</param>
        /// <returns>A Geohash object.</returns>
        public static Geohash Parse(SqlString s)
        {
            if (s.IsNull)
                return Null;
            if (s.Value.Length > 12)
                throw new ArgumentException("Geohash string too long");
            for (int i = 0; i < s.Value.Length; i++)
            {
                if (base32.IndexOf(s.Value[i]) == -1)
                    throw new ArgumentException("Invalid character in geohash string");
            }
            return new Geohash(s.Value);
        }
        
        /// <summary>
        /// Compares the current Geohash object with another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>A value that indicates the order of the objects being compared.</returns>
        public int CompareTo(object obj)
        {
            if (obj is Geohash)
            {
                Geohash h = (Geohash)obj;
                return value.CompareTo(h.value);
            }
            else
            {
                throw new ArgumentException("Cant compare Geohash to " + obj.GetType().ToString());
            }
        }
        
        /// <summary>
        /// Gets the string value of the Geohash.
        /// </summary>
        public string Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Encodes the specified coordinates into a Geohash object with the specified precision.
        /// </summary>
        /// <param name="X">The longitude coordinate.</param>
        /// <param name="Y">The latitude coordinate.</param>
        /// <param name="precision">The precision of the Geohash.</param>
        /// <returns>A Geohash object.</returns>
        public static Geohash Encode(double X, double Y, int precision)
        {
            return new Geohash(X, Y, precision);
        }

        /// <summary>
        /// Decodes the Geohash into an array of coordinates.
        /// </summary>
        /// <returns>An array of coordinates representing the bounding box.</returns>
        public double[] Decode()
        {
            if (_null)
                return null;

            double[] lon = new double[2] { -180, 180 };
            double[] lat = new double[2] { -90, 90 };

            int ch = 0;
            int j = 0;
            for (int i = 0; i < value.Length * 5; i++)
            {
                if (i % 5 == 0)
                    ch = base32.IndexOf(value[j++]);
                if (i % 2 == 0)
                {
                    double mid = (lon[0] + lon[1]) / 2;
                    if ((ch & 16) == 16)
                    {
                        lon[0] = mid;
                    }
                    else
                        lon[1] = mid;
                } 
                else
                {
                    double mid = (lat[0] + lat[1]) / 2;
                    if ((ch & 16) == 16)
                    {
                        lat[0] = mid;
                    }
                    else
                        lat[1] = mid;
                }
                ch <<= 1;
            }
            double[] result = new double[4];
            result[0] = lon[0];
            result[1] = lon[1];
            result[2] = lat[0];
            result[3] = lat[1];
            return result;
        }
        
        /// <summary>
        /// Determines whether the current Geohash includes the specified Geohash.
        /// </summary>
        /// <param name="other">The Geohash to compare.</param>
        /// <returns>true if the current Geohash includes the specified Geohash.<returns>
        public bool Includes(Geohash other)
        {
            if (IsNull || other.IsNull)
                return false;
            if (other.value.StartsWith(value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether the current Geohash is visible from the specified Geohash.
        /// </summary>
        /// <param name="other">The Geohash to compare.</param>
        /// <returns>true if the current Geohash is visible from the specified Geohash.<returns>
        public bool VisibleFrom(Geohash other)
        {
            if (IsNull || other.IsNull)
                return false;
            int c = other.Common(this).value.Length;
            if (c == value.Length || (c == other.value.Length && value.Length > other.value.Length))
                return true;
            else
                return false;
        } 

        /// <summary>
        /// Returns the common prefix of the current Geohash and the specified Geohash.
        /// </summary>
        /// <param name="other">The Geohash to compare.</param>
        /// <returns>A Geohash object representing the common prefix.</returns>
        public Geohash Common(Geohash other)
        {
            if (IsNull || other.IsNull)
                return Null;
            int i = 0;
            while (i < value.Length && i < other.value.Length && value[i] == other.value[i])
                i++;
            return new Geohash(value.Substring(0, i));
        }

        /// <summary>
        /// Calculates the size of the Geohash in the X direction.
        /// </summary>
        /// <param name="precision">The precision of the Geohash.</param>
        /// <returns>The size of the Geohash in the X direction.</returns>
        public static double size_x (int precision)
        {
            int lat_bits = 5 * precision / 2;
            int lon_bits = 5 * precision - lat_bits;
            return 360.0 / (1 << lon_bits);
        }

        /// <summary>
        /// Calculates the size of the Geohash in the Y direction.
        /// </summary>
        /// <param name="precision">The precision of the Geohash.</param>
        /// <returns>The size of the Geohash in the Y direction.</returns>
        public static double size_y (int precision)
        {
            int lat_bits = 5 * precision / 2;
            return 180.0 / (1 << lat_bits);
        }

        /// <summary>
        /// Reads the Geohash from the specified BinaryReader.
        /// </summary>
        /// <param name="r">The BinaryReader to read from.</param>
        public void Read(BinaryReader r)
        {
            value = r.ReadString();
            _null = r.ReadBoolean();
        }

        /// <summary>
        /// Writes the Geohash to the specified BinaryWriter.
        /// </summary>
        /// <param name="w">The BinaryWriter to write to.</param>
        public void Write(BinaryWriter w)
        {
            w.Write(value);
            w.Write(_null);
        }
    }
}