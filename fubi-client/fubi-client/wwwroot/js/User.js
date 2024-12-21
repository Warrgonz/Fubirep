
function previewImage(event) {
    const file = event.target.files[0];
    const reader = new FileReader();

    reader.onload = function (e) {
        // Cambiar la fuente de la imagen al archivo cargado
        document.getElementById('profilePic').src = e.target.result;
    };

    // Leer el archivo como una URL de imagen
    if (file) {
        reader.readAsDataURL(file);
    }
}





