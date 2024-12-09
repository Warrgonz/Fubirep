// Data table con los beneficiarios
$(document).ready(function () {
    const beneficiarios = [
        { id: 1, cedula: 12345678, beneficiario: "Juan Pérez", correo: "juan@mail.com", telefono: "88888888", direccion: "San José", activo: true },
        { id: 2, cedula: 87654321, beneficiario: "Ana López", correo: "ana@mail.com", telefono: "77777777", direccion: "Heredia", activo: false },
    ];

    $('#user-table').DataTable({
        data: beneficiarios.map(b => [
            b.id,
            b.cedula,
            b.beneficiario,
            b.correo,
            b.telefono,
            b.direccion,
            b.activo ? "Activo" : "Inactivo"
        ]),
        columns: [
            { title: "#" },
            { title: "Cédula" },
            { title: "Beneficiario" },
            { title: "Correo" },
            { title: "Teléfono" },
            { title: "Dirección" },
            { title: "Estado" },
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.5/i18n/es-ES.json"
        }
    });
});
