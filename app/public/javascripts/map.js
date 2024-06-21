const canvas = document.getElementById('canvas');
var ctx = canvas.getContext('2d');
var x, y;
var zoom;

export function update(Pos, Zoom) {
    x = Pos[0];
    y = Pos[1];
    zoom = Zoom;
    drawAll();
    fetch_data();
}

var Points = [];
var Polygons = [];

function drawAll() {
    ctx.fillStyle = 'lightblue';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    drawBorder();
    Polygons.forEach(polygon => {
        drawPolygon(polygon);
    });
    Points.forEach(point => {
        drawPoint(point);
    });
}

function drawBorder() {
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    ctx.strokeRect(zoom * (-180 + x) + canvas.width / 2, zoom * (-90 + y) + canvas.height / 2, 360 * zoom, 180 * zoom);
}

function drawPolygon(polygon) {
    ctx.fillStyle = `rgba(${polygon.color}, 0.7)`;
    ctx.beginPath();
    ctx.moveTo(zoom * (polygon.points[0][0] + x) + canvas.width / 2, zoom * (-polygon.points[0][1] + y) + canvas.height / 2);
    for (let i = 1; i < polygon.points.length; i++) {
        ctx.lineTo(zoom * (polygon.points[i][0] + x) + canvas.width / 2, zoom * (-polygon.points[i][1] + y) + canvas.height / 2);
    }
    ctx.closePath();
    ctx.fill();
}

function drawPoint(point) {
    ctx.fillStyle = `rgba(${point.color}, 0.9)`;
    ctx.beginPath();
    let xpos = zoom * (point.pos[0] + x) + canvas.width / 2;
    let ypos = zoom * (-point.pos[1] + y) + canvas.height / 2;
    ctx.arc(xpos, ypos, 2, 0, 2 * Math.PI);
    ctx.fill();
    ctx.font = "Roboto 12px bold-serif";
    ctx.fillText(`p(${point.point_id}) ${point.name}`, xpos + 10, ypos + 10);
}


var lock = false;
async function fetch_data() {
    if (!lock) {
        lock = true;
        var xmin = -x - canvas.width / 2 / zoom
        var ymin = y - canvas.height / 2 / zoom
        var xmax = -x + canvas.width / 2 / zoom
        var ymax = y + canvas.height / 2 / zoom
        fetch('/map', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ xmin: xmin, ymin: ymin, xmax: xmax, ymax: ymax }) })
            .then(res => res.json()).then(data => {
                Points = data.points.map(point => {
                    const parser = new DOMParser();
                    const data = parser.parseFromString(point.data, "text/xml");
                    let name = data.getElementsByTagName("name")[0].textContent;
                    let color = data.getElementsByTagName("color")[0];
                    let r = color.getElementsByTagName("r")[0].textContent;
                    let g = color.getElementsByTagName("g")[0].textContent;
                    let b = color.getElementsByTagName("b")[0].textContent;
                    let pos = point.pos.split(',').map(p => parseFloat(p))
                    return { point_id: point.point_id, pos: pos, name: name, color: `${r}, ${g}, ${b}` };
                });

                Polygons = data.polygons.map(polygon => {
                    const parser = new DOMParser();
                    const data = parser.parseFromString(polygon.data, "text/xml");
                    let name = data.getElementsByTagName("name")[0].textContent;
                    let color = data.getElementsByTagName("color")[0];
                    let r = color.getElementsByTagName("r")[0].textContent;
                    let g = color.getElementsByTagName("g")[0].textContent;
                    let b = color.getElementsByTagName("b")[0].textContent;
                    let points = polygon.points.split(';').map(p => {
                        p = p.split(',').map(p => parseFloat(p))
                        return p;
                    });
                    points.pop();
                    return { polygon_id: polygon.polygon_id, points: points, name: name, color: `${r}, ${g}, ${b}` };
                });
                drawAll();
            }).finally(() => { lock = false; });
    }
}