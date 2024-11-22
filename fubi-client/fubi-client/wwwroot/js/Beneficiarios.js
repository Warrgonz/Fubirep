// Data table con los beneficiarios
$(document).ready(function () {
    $('#beneficiario-table').DataTable({
        data: [],
        columns: [
            { title: "#" },
            { title: "Cédula" },
            { title: "Beneficiario" },
            { title: "Correo" },
            { title: "Teléfono" },
            { title: "Dirección" },
            { title: "Acciones" }
        ],
        language: {
            emptyTable: "No hay datos disponibles en la tabla",
            url: "https://cdn.datatables.net/plug-ins/1.13.5/i18n/es-ES.json"
        }
    });
});
// Mostrar el preview de la imagen del beneficiario.
function previewBeneficiarioImage(event) {
    const file = event.target.files[0];
    const reader = new FileReader();

    reader.onload = function (e) {
        // Cambiar la fuente de la imagen al archivo cargado
        document.getElementById('beneficiarioPic').src = e.target.result;
    };

    // Leer el archivo como una URL de imagen
    if (file) {
        reader.readAsDataURL(file);
    }
}
