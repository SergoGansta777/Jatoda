namespace Jatoda.Services.MinIoService.Interfaces;

public interface IMinioService
{
    Task UploadFileAsync(string bucketName, string objectName, Stream data);
    Task<string> GetFileUrlAsync(string bucketName, string objectName);
    Task<Stream?> GetObjectAsync(string bucketName, string objectName);
}