'use strict';

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

function elHechicero() {
    var passwordInput = document.getElementById('passwordInput');
    var eyeIcon = document.getElementById('eyeIcon');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        eyeIcon.name = 'eye-off-outline';
    } else {
        passwordInput.type = 'password';
        eyeIcon.name = 'eye-outline';
    }
}