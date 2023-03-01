function createObjectURLFromBA(byteArray) {
    var blob = new Blob([byteArray]);
    var blobURL = URL.createObjectURL(blob);

    return blobURL;
}

function windowDevicePixelRatio() {
    return window.devicePixelRatio;
}

class WindowDPRHelper {
    static dotNetHelper;

    static callupdatePixelRatio(value) {
        WindowDPRHelper.dotNetHelper = value;
        WindowDPRHelper.updatePixelRatio();
    }

    static updatePixelRatio = () => {
        let pr = window.devicePixelRatio;
        matchMedia(`(resolution: ${pr}dppx)`).addEventListener("change", WindowDPRHelper.updatePixelRatio, { once: true });
        WindowDPRHelper.dotNetHelper.invokeMethodAsync('SetWindowDPR', pr);
    }
}

window.WindowDPRHelper = WindowDPRHelper;

function onKeyPressed(e) {
    if (!e.repeat) {
        WindowDPRHelper.dotNetHelper.invokeMethodAsync('OnKeyPressedJS', e.ctrlKey, e.shiftKey, e.code);
    }
    //console.log(e.code);
};

function attachHandlers() {
    document.removeEventListener('keydown', onKeyPressed);
    document.addEventListener('keydown', onKeyPressed);
};

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

function median(numbers) {
    var median = 0,
        numsLen = numbers.length;
    numbers.sort();

    if (
        numsLen % 2 === 0
    ) {
        median = Math.round((numbers[numsLen / 2 - 1] + numbers[numsLen / 2]) / 2);
    } else {
        median = numbers[(numsLen - 1) / 2];
    }

    return median;
}

function medianFilter(image, imWidth, imHeight, radius) {
    //let width = image.width;
    //let height = image.height;
    //var input = image.getImageData(0, 0, image.width, image.height).data;
    //var output = input;
    let width = imWidth;
    let height = imHeight;
    var input = image;
    var output = input;
    for (var x = 0; x < width; x++) {
        for (var y = 0; y < height; y++) {
            var index = (x + y * width) * 4;
            var bufferRed = [];
            var bufferGreen = [];
            var bufferYellow = [];
            //var bufferAlpha = [];
            for (var cx = 0; cx < radius; cx++) {
                for (var cy = 0; cy < radius; cy++) {
                    if (x + cx < width && y + cy < height) {
                        var idx = (x + cx + (y + cy) * width) * 4;
                        bufferRed.push(input[idx]);
                        bufferGreen.push(input[idx + 1]);
                        bufferYellow.push(input[idx + 2]);
                        //bufferAlpha.push(input[idx + 3]);
                    }
                }
            }
            output[index] = median(bufferRed.sort());
            output[index + 1] = median(bufferGreen.sort());
            output[index + 2] = median(bufferYellow.sort());
            //output[index + 3] = median(bufferAlpha.sort());
        }
    }
    return output;
}