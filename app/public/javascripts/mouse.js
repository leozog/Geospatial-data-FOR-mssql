let mouseDown = false;
let mouseClickPos = [0, 0];

const canvas = document.getElementById('canvas');
export function mouseCanvasPos(event) {
    return [
        event.clientX - canvas.offsetLeft - canvas.width / 2,
        event.clientY - canvas.offsetTop - canvas.height / 2
    ];
}

export function mouseDelta(event) {
    var pos = mouseCanvasPos(event);
    return [(pos[0] - mouseClickPos[0]), (pos[1] - mouseClickPos[1])];
}

addEventListener('mousedown', (event) => {
    mouseDown = true;
    mouseClickPos = mouseCanvasPos(event);
});

addEventListener('mouseup', (event) => {
    mouseDown = false;
});

export function isMouseDown() {
    return mouseDown;
}