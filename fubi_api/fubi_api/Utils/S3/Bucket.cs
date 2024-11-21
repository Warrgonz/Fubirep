using Amazon.S3;
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

    }
}
