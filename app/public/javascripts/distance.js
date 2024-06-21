function getDistance() {
    var id1 = parseInt(document.getElementById('distance_id1').value);
    var id2 = parseInt(document.getElementById('distance_id2').value);
    fetch('/distance', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id1: id1, id2: id2 })
    }).then(response => response.json()).then(data => {
        document.getElementById('distance').innerHTML = data[0].distance;
    });
}
document.getDistance = getDistance;