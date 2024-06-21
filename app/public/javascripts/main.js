import { mouseCanvasPos, mouseDelta, isMouseDown } from './mouse.js';
import { update } from './map.js';

var camPos = [0, 0];
var camZoom = 1;

addEventListener('mousemove', (event) => {
    if (isMouseDown()) {
        update(
            [
                camPos[0] + mouseDelta(event)[0] / (2 ** camZoom),
                camPos[1] + mouseDelta(event)[1] / (2 ** camZoom)
            ],
            2 ** camZoom
        );
    }
});

canvas.addEventListener('wheel', (event) => {
    camZoom += event.deltaY * -0.001;
    if (isMouseDown()) {
        update(
            [
                camPos[0] + mouseDelta(event)[0] / (2 ** camZoom),
                camPos[1] + mouseDelta(event)[1] / (2 ** camZoom)
            ],
            2 ** camZoom
        );
    } else {
        update(camPos, 2 ** camZoom);
    }
});

addEventListener('mouseup', (event) => {
    camPos = [
        camPos[0] + mouseDelta(event)[0] / (2 ** camZoom),
        camPos[1] + mouseDelta(event)[1] / (2 ** camZoom)
    ],
        update(camPos, 2 ** camZoom);
});

update(camPos, 2 ** camZoom); document.onP

export function mouseReal(event) {
    return [
        mouseCanvasPos(event)[0] / (2 ** camZoom) - camPos[0],
        -mouseCanvasPos(event)[1] / (2 ** camZoom) + camPos[1]
    ];
}