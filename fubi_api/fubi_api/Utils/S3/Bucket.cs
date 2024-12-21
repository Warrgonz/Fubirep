using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace fubi_api.Utils.S3
{
    public class Bucket : IBucket
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        // Constructor que recibe la configuración
        public Bucket(IConfiguration configuration)
        {
            _s3Client = new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                Amazon.RegionEndpoint.USEast1
            );

            _bucketName = "fubiredip-s3";
        }

        // Método para subir archivos a S3
        public async Task<string> UploadFileAsync(IFormFile file, string type, string id)
        {
            if (file.Length == 0)
                throw new ArgumentException("El archivo está vacío.");

            // Genera un nombre único para el archivo
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Define el "directorio" o ruta en S3
            var key = $"{type}/{id}/{fileName}";

            // Crear una solicitud de subida a S3
            var fileTransferUtility = new TransferUtility(_s3Client);

            var fileUploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = file.OpenReadStream(),
                CannedACL = S3CannedACL.PublicRead
            };

            try
            {
                // Subimos el archivo al bucket S3
                await fileTransferUtility.UploadAsync(fileUploadRequest);

                // Retornamos la URL pública del archivo subido
                var fileUrl = $"https://{_bucketName}.s3.amazonaws.com/{key}";
                return fileUrl;
            }
            catch (Exception ex)
            {
                // Si ocurre un error, lanzamos una excepción
                throw new Exception("Error al subir el archivo a S3.", ex);
            }
        }

        public async Task<string> UpdateFileAsync(IFormFile file, string type, string id)
        {
            if (file.Length == 0)
                throw new ArgumentException("El archivo está vacío.");

            // Generar prefijo para los archivos asociados a esta cédula
            var prefix = $"{type}/{id}/";

            try
            {
                // Listar los archivos existentes asociados al prefijo
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                var listResponse = await _s3Client.ListObjectsV2Async(listRequest);

                // Eliminar los archivos existentes
                if (listResponse.S3Objects.Count > 0)
                {
                    var deleteObjectsRequest = new DeleteObjectsRequest
                    {
                        BucketName = _bucketName
                    };

                    deleteObjectsRequest.Objects.AddRange(listResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }));

                    await _s3Client.DeleteObjectsAsync(deleteObjectsRequest);
                }

                // Subir el nuevo archivo
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var key = $"{type}/{id}/{fileName}";

                var fileTransferUtility = new TransferUtility(_s3Client);
                var fileUploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                    CannedACL = S3CannedACL.PublicRead
                };

                await fileTransferUtility.UploadAsync(fileUploadRequest);

                // Retornar la URL pública del nuevo archivo
                var fileUrl = $"https://{_bucketName}.s3.amazonaws.com/{key}";
                return fileUrl;
            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception("Error al interactuar con S3.", s3Ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error general al procesar el archivo.", ex);
            }
        }


    }
}
