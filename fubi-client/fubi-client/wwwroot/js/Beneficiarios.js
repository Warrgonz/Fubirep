$(document).ready(function () {
    $('#beneficiarios-table').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.5/i18n/es-ES.json'
        },
        columnDefs: [
            { className: "dt-head-center", targets: "_all" } // Centra los encabezados de todas las columnas
        ]
    });
});

// Función para desactivar un beneficiario
document.addEventListener('DOMContentLoaded', () => {
    const beneficiariosTable = document.querySelector('#beneficiarios-table');

    if (!beneficiariosTable) {
        console.error('Error: No se encontró la tabla con ID "beneficiarios-table".');
        return;
    }

    beneficiariosTable.addEventListener('click', async (event) => {
        if (event.target.classList.contains('btn-desactivar')) {
            const id = event.target.dataset.id;

            if (!id) {
                console.error('Error: No se encontró el ID del beneficiario en el atributo data-id.');
                return;
            }

            console.log(`Intentando desactivar beneficiario con ID: ${id}`);

            const result = await Swal.fire({
                title: `¿Deseas desactivar al beneficiario con ID ${id}?`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, desactivar',
                cancelButtonText: 'Cancelar',
            });

            if (result.isConfirmed) {
                try {
                    console.log(`Enviando solicitud a: /Beneficiarios/Desactivar/${id}`);
                    const response = await fetch(`/Beneficiarios/Desactivar/${id}`, {
                        method: 'POST',
                    });

                    if (!response.ok) {
                        console.error(`Error en la respuesta del servidor. Código: ${response.status}`);
                        Swal.fire({
                            title: 'Error',
                            text: `Ocurrió un error en el servidor (Código: ${response.status})`,
                            icon: 'error',
                        });
                        return;
                    }

                    const data = await response.json();
                    console.log('Respuesta del servidor:', data);

                    if (data && data.codigo === 0) {
                        Swal.fire({
                            title: '¡Éxito!',
                            text: 'Beneficiario desactivado exitosamente.',
                            icon: 'success',
                        });
                        location.reload();
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: data.mensaje || 'No se pudo desactivar el beneficiario.',
                            icon: 'error',
                        });
                    }
                } catch (error) {
                    console.error('Error de red o en la solicitud:', error);
                    Swal.fire({
                        title: 'Error',
                        text: 'Ocurrió un problema al conectar con el servidor.',
                        icon: 'error',
                    });
                }
            }
        }
    });
});
