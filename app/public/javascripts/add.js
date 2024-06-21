function addPoint() {
    var point = document.getElementById('points').value;
    var name = document.getElementById('name').value;
    var r = document.getElementById('color_r').value;
    var g = document.getElementById('color_g').value;
    var b = document.getElementById('color_b').value;
    var xml_data = `<data><name>${name}</name><color><r>${r}</r><g>${g}</g><b>${b}</b></color></data>`;
    fetch('/add_point', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ point: point, data: xml_data })
    }).then(response => response.json()).then(data => {
        if (data.success) {
            document.getElementById('result').innerHTML = data.success;
        } else {
            document.getElementById('result').innerHTML = data.error;
        }
    });
}
document.addPoint = addPoint;

function addPolygon() {
    var points = document.getElementById('points').value;
    var name = document.getElementById('name').value;
    var r = document.getElementById('color_r').value;
    var g = document.getElementById('color_g').value;
    var b = document.getElementById('color_b').value;
    var xml_data = `<data><name>${name}</name><color><r>${r}</r><g>${g}</g><b>${b}</b></color></data>`;
    fetch('/add_polygon', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ points: points, data: xml_data })
    }).then(response => response.json()).then(data => {
        if (data.success) {
            document.getElementById('result').innerHTML = data.success;
        } else {
            console.log(data.error);
            document.getElementById('result').innerHTML = data.error;
        }
    });
}
document.addPolygon = addPolygon;