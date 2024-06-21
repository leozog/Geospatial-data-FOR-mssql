var express = require('express');
var router = express.Router();
var map = require('@models/map.js');

/* GET home page. */
router.get('/', function (req, res, next) {
  res.render('frontpage', {});
});

router.post('/map', map.map_get);
router.post('/inside', map.inside);
router.post('/distance', map.distance);

router.post('/add_point', map.add_point);
router.post('/add_polygon', map.add_polygon);

module.exports = router;
