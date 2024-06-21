using System;
using System.Data.SqlTypes;
using System.IO;
using NUnit.Framework;
using bd2prj;

namespace bd2prj_utest
{
    [TestFixture]
    public class GeohashTests
    {
        [Test]
        public void TestGeohashNull()
        {
            var nullGeohash = Geohash.Null;
            Assert.IsTrue(nullGeohash.IsNull);
            Assert.AreEqual("NULL", nullGeohash.ToString());
        }

        [Test]
        public void TestGeohashConstructor_String()
        {
            var geohash = new Geohash("u4pruydqqvj");
            Assert.IsFalse(geohash.IsNull);
            Assert.AreEqual("u4pruydqqvj", geohash.Value);
        }

        [Test]
        public void TestGeohashConstructor_Coordinates()
        {
            var geohash = new Geohash(115.99999996, 10.99999993, 12);
            Assert.IsFalse(geohash.IsNull);
            Assert.AreEqual("w9fqxdkxeut5", geohash.Value);
        }

        [Test]
        public void TestParse_InvalidString()
        {
            Assert.Throws<ArgumentException>(() => Geohash.Parse(new SqlString("AAA")));
        }

        [Test]
        public void TestParse_NullString()
        {
            var geohash = Geohash.Parse(SqlString.Null);
            Assert.IsTrue(geohash.IsNull);
        }

        [Test]
        public void TestEncode()
        {
            var geohash = Geohash.Encode(57.64911, 10.40744, 12);
            Assert.IsFalse(geohash.IsNull);
            Assert.AreEqual("t3bgpydg9xkj", geohash.Value);
        }

        [Test]
        public void TestDecode()
        {
            var geohash = new Geohash("u4pruydqqvj8");
            var decoded = geohash.Decode();
            Assert.AreEqual(10.40743986, decoded[0], 1e-6);
            Assert.AreEqual(57.64911004, decoded[2], 1e-6);
        }

        [Test]
        public void TestIncludes()
        {
            var geohash = new Geohash("u4pruy");
            var other = new Geohash("u4pruyd");
            Assert.IsTrue(geohash.Includes(other));
            Assert.IsFalse(other.Includes(geohash));
        }

        [Test]
        public void TestVisibleFrom1()
        {
            var g1 = new Geohash("u4pruyd");
            var g2 = new Geohash("u4pru");
            Assert.IsTrue(g1.VisibleFrom(g2));
            Assert.IsTrue(g2.VisibleFrom(g1));
        }

        [Test]
        public void TestVisibleFrom2()
        {
            var g1 = new Geohash("u4pruyd");
            var g2 = new Geohash("u4pr7");
            Assert.IsFalse(g1.VisibleFrom(g2));
            Assert.IsFalse(g2.VisibleFrom(g1));
        }

        [Test]
        public void TestCommon()
        {
            var geohash = new Geohash("u4pruyd");
            var other = new Geohash("u4pruy");
            var common = geohash.Common(other);
            Assert.AreEqual("u4pruy", common.Value);
        }
    }
}