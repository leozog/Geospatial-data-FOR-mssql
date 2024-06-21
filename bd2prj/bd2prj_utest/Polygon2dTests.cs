using System;
using System.Data.SqlTypes;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using bd2prj;


namespace bd2prj_utest
{
    [TestFixture]
    public class Polygon2dTests
    {
        private Point2d point1, point2, point3;
        private Polygon2d polygon;

        [SetUp]
        public void Setup()
        {
            point1 = new Point2d(0, 0);
            point2 = new Point2d(1, 0);
            point3 = new Point2d(0, 1);
            polygon = new Polygon2d(new List<Point2d> { point1, point2, point3 });
        }

        [Test]
        public void TestNullPolygon()
        {
            Polygon2d nullPolygon = Polygon2d.Null;
            Assert.IsTrue(nullPolygon.IsNull);
        }

        [Test]
        public void TestToString()
        {
            string expected = "0,0;1,0;0,1;";
            Assert.AreEqual(expected, polygon.ToString());
        }

        [Test]
        public void TestParse1()
        {
            string input = "0,0;1,0;0,1;";
            Polygon2d parsedPolygon = Polygon2d.Parse(new SqlString(input));
            Assert.AreEqual(input, parsedPolygon.ToString());
        }

        [Test]
        public void TestParse2()
        {
            string input = "0,0; 1,0;0, 1";
            Polygon2d parsedPolygon = Polygon2d.Parse(new SqlString(input));
            Assert.AreEqual("0,0;1,0;0,1;", parsedPolygon.ToString());
        }

        [Test]
        public void TestVisibleFrom()
        {
            Assert.IsTrue(polygon.VisibleFrom(-1, -1, 2, 2));
            Assert.IsFalse(polygon.VisibleFrom(-1, -1, -0.5, -0.5));
        }

        [Test]
        public void TestArea()
        {
            double expectedArea = 0.5;
            Assert.AreEqual(expectedArea, polygon.Area);
        }

        [Test]
        public void TestIncludesPoint()
        {
            Point2d insidePoint = new Point2d(0.5, 0.5);
            Point2d outsidePoint = new Point2d(2, 2);
            Assert.IsTrue(polygon.Includes(insidePoint));
            Assert.IsFalse(polygon.Includes(outsidePoint));
        }
    }
}
