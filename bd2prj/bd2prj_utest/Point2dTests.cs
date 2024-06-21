using System;
using System.Data.SqlTypes;
using System.IO;
using NUnit.Framework;
using bd2prj;

namespace bd2prj_utest
{
    [TestFixture]
    public class Point2dTests
    {
        [Test]
        public void TestNullProperty()
        {
            var point = Point2d.Null;
            Assert.IsTrue(point.IsNull);
        }

        [Test]
        public void TestToString()
        {
            var point = new Point2d(1.5, 2.5);
            Assert.AreEqual("1.5,2.5", point.ToString());
        }

        [Test]
        public void TestToStringNull()
        {
            var point = Point2d.Null;
            Assert.AreEqual("NULL", point.ToString());
        }

        [Test]
        public void TestParse()
        {
            var point = Point2d.Parse(new SqlString("1.5, 2.5"));
            Assert.AreEqual(1.5, point.X);
            Assert.AreEqual(2.5, point.Y);
        }

        [Test]
        public void TestParseInvalidString()
        {
            Assert.Throws<ArgumentException>(() => Point2d.Parse(new SqlString("1.5")));
        }

        [Test]
        public void TestParseInvalidFormat()
        {
            Assert.Throws<FormatException>(() => Point2d.Parse(new SqlString("1.5, abc")));
        }

        [Test]
        public void TestVisibleFrom()
        {
            var point = new Point2d(5, 5);
            Assert.IsTrue(point.VisibleFrom(0, 0, 10, 10));
            Assert.IsFalse(point.VisibleFrom(6, 6, 10, 10));
        }

        [Test]
        public void TestDist()
        {
            var point1 = new Point2d(0, 0);
            var point2 = new Point2d(3, 4);
            Assert.AreEqual(5, point1.Dist(point2));
        }

        [Test]
        public void TestDistWithNull()
        {
            var point1 = new Point2d(0, 0);
            var point2 = Point2d.Null;
            Assert.AreEqual(double.NaN, point1.Dist(point2));
        }

        [Test]
        public void TestSetXY()
        {
            var point = new Point2d();
            point.SetXY(1.1, 2.2);
            Assert.AreEqual(1.1, point.X);
            Assert.AreEqual(2.2, point.Y);
        }
    }
}
