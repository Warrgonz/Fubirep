function sleep(time) {
    return new Promise((resolve) => setTimeout(resolve, time))
}
(() => {
    let alert = document.getElementById('error-message');
    if (alert != null) {
        sleep(5000).then(() => {
            alert.style.display = 'none';
        });
    }
})()