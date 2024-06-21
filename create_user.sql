CREATE LOGIN AppUser WITH PASSWORD = 'Haslo!123!&';
USE bd2prj;
CREATE USER AppUser FOR LOGIN AppUser;
ALTER ROLE db_owner ADD MEMBER AppUser;

Drop table point;
Drop table polygon;

Create table point(
	point_id INT IDENTITY(1,1) PRIMARY KEY,
	p Point2d,
	[data] xml
);
CREATE INDEX IX_Geohash ON point(p);


Create table polygon(
	polygon_id INT IDENTITY(1,1) PRIMARY KEY,
	p dbo.Polygon2d,
	geohashMax Geohash,
	geohashMin Geohash,
	area float,
	[data] xml
);
GO
CREATE TRIGGER trg_UpdateGeohashes
ON polygon
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE polygon
    SET 
        geohashMax = polygon.p.GeohashMax,
        geohashMin = polygon.p.GeohashMin,
		area = polygon.p.Area
    FROM 
        polygon
    INNER JOIN
        inserted ON polygon.polygon_id = inserted.polygon_id;
END;
GO
CREATE INDEX IX_geohashMax ON polygon(geohashMax);
CREATE INDEX IX_geohashMin ON polygon(geohashMin);
CREATE INDEX IX_area ON polygon(area);