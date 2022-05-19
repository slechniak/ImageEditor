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