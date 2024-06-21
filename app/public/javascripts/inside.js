import { mouseReal } from './main.js';

addEventListener('mousemove', (event) => {
    var inside = document.getElementById('inside');
    fetch_inide(mouseReal(event), inside);
});

var lock = false;
function fetch_inide(pos, inside) {
    if (!lock) {
        lock = true;
        fetch('/inside', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ x: pos[0], y: pos[1] })
        }).then(response => response.json()).then(data => {
            inside.innerHTML = data.map(polygon => {
                const parser = new DOMParser();
                const data = parser.parseFromString(polygon.data, "text/xml");
                let name = data.getElementsByTagName("name")[0].textContent;
                let color = data.getElementsByTagName("color")[0];
                let r = color.getElementsByTagName("r")[0].textContent;
                let g = color.getElementsByTagName("g")[0].textContent;
                let b = color.getElementsByTagName("b")[0].textContent;
                return `polygon_id: ${polygon.polygon_id}, area: ${polygon.area}, name: ${name}, color: (${r}, ${g}, ${b})`;
            }).join('; ');
        }).finally(() => {
            lock = false;
        });
    }
}