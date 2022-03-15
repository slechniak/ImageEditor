function createObjectURLFromBA(byteArray) {
    var blob = new Blob([byteArray]);
    var blobURL = URL.createObjectURL(blob);

    return blobURL;
}