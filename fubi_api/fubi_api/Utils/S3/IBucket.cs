namespace fubi_api.Utils.S3
{
    public interface IBucket
    {
        Task<string> UploadFileAsync(IFormFile file, string type, string id);
    }
}
