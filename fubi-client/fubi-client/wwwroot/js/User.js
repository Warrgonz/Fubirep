// Data table con los usuarios
$(document).ready(function () {
    $('#user-table').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/2.1.8/i18n/es-ES.json'
        }
        { className: "dt-head-center", targets: [0] }
    });
});

// Función para eliminar un usuario
document.addEventListener('DOMContentLoaded', () => {
    const userTable = document.querySelector('#user-table');

    if (!userTable) {
        console.log('Error: No se encontró la tabla con ID "user-table".');
        return;
    }

    userTable.addEventListener('click', async (event) => {
        if (event.target.classList.contains('btn-desactivar')) {
            const cedula = event.target.dataset.cedula;

            if (!cedula) {
                console.log('Error: No se encontró la cédula en el atributo data-cedula.');
                return;
            }

            console.log(`Intentando desactivar usuario con cédula: ${cedula}`);

            const result = await Swal.fire({
                title: `¿Deseas desactivar al usuario con cédula ${cedula}?`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, desactivar',
                cancelButtonText: 'Cancelar',
            });

            if (result.isConfirmed) {
                try {
                    console.log(`Enviando solicitud a: /User/DesactivarUsuario/${cedula}`);
                    const response = await fetch(`/User/DesactivarUsuario/${cedula}`, {
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
                            text: 'Usuario desactivado exitosamente.',
                            icon: 'success',
                        });
                        location.reload();
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: data.mensaje || 'No se pudo desactivar el usuario.',
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



// Mostrar el preview de la imagen del usuario.

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

