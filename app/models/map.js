var db = require('@utility/database');


exports.map_get = async function (req, res, next) {
    var xmin = req.body.xmin;
    var ymin = req.body.ymin;
    var xmax = req.body.xmax;
    var ymax = req.body.ymax;
    var points = new Promise((resolve, reject) => {
        db.Request()
            .input('xmin', xmin)
            .input('ymin', ymin)
            .input('xmax', xmax)
            .input('ymax', ymax)
            .query(`SELECT r.point_id, p.ToString() as pos, r.data as data 
                    FROM points_in_window(@xmin, @ymin, @xmax, @ymax) q 
                    JOIN point r ON q.point_id = r.point_id`)
            .then((result) => {
                resolve(result.recordset);
            });
    });
    var min_area = 0.002 * (xmax - xmin) * (ymax - ymin);
    var polygons = new Promise((resolve, reject) => {
        db.Request()
            .input('xmin', xmin)
            .input('ymin', ymin)
            .input('xmax', xmax)
            .input('ymax', ymax)
            .input('min_area', min_area)
            .query(`SELECT r.polygon_id, p.ToString() as points, r.data as data
                    FROM polygons_in_window(@xmin, @ymin, @xmax, @ymax, @min_area) q 
                    JOIN polygon r ON q.polygon_id = r.polygon_id`)
            .then((result) => {
                resolve(result.recordset);
            });
    });
    Promise.all([points, polygons]).then((values) => {
        res.json({ points: values[0], polygons: values[1] });
    });
}

exports.inside = async function (req, res, next) {
    db.Request()
        .input('x', req.body.x)
        .input('y', req.body.y)
        .query(`SELECT r.polygon_id, p.Area as area, r.data as data
                FROM polygon_includes(@x, @y, 0) q
                JOIN polygon r ON q.polygon_id = r.polygon_id`)
        .then((result) => {
            res.json(result.recordset);
        });
}

exports.distance = async function (req, res, next) {
    db.Request()
        .input('id1', req.body.id1)
        .input('id2', req.body.id2)
        .query(`SELECT p.Dist((SELECT p FROM point WHERE point_id = @id2)) as distance 
                FROM point WHERE point_id = @id1`)
        .then((result) => {
            res.json(result.recordset);
        });
}

exports.add_point = function (req, res, next) {
    db.Request()
        .input('pos', req.body.point)
        .input('data', req.body.data)
        .query('INSERT INTO point(p, data) VALUES(CONVERT(dbo.Point2d, @pos), @data)')
        .then(() => {
            res.json({ success: true }).send();
        })
        .catch((err) => {
            console.log('err' + err);
            res.json({ success: false, error: err.message }).send();
        });
}

exports.add_polygon = function (req, res, next) {
    db.Request()
        .input('points', req.body.points)
        .input('data', req.body.data)
        .query('INSERT INTO polygon(p, data) VALUES(CONVERT(dbo.Polygon2d, @points), @data)')
        .then(() => {
            res.json({ success: true }).send();
        })
        .catch((err) => {
            res.json({ success: false, error: err.message }).send();
        });
}