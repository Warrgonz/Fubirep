// Inicializar DataTable con datos de beneficiarios
$(document).ready(function () {
    $('#beneficiario-table').DataTable({
        ajax: {
            url: '/Beneficiarios/GetAll', // Endpoint para obtener los datos
            dataSrc: '' // Adaptar según el formato del JSON recibido
        },
        columns: [
            { data: "Id", title: "#" },
            { data: "Cedula", title: "Cédula" },
            { data: "Beneficiario", title: "Beneficiario" },
            { data: "Correo", title: "Correo" },
            { data: "Telefono", title: "Teléfono" },
            { data: "Direccion", title: "Dirección" },
            {
                data: null,
                title: "Acciones",
                render: function (data) {
                    return `
                        <a class="btn btn-sm btn-warning" href="/Beneficiarios/Edit/${data.Id}">Editar</a>
                        <button class="btn btn-sm btn-danger" onclick="deleteBeneficiario(${data.Id})">Eliminar</button>`;
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.5/i18n/es-ES.json"
        }
    });
});
