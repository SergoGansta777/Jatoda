namespace JatodaBackendApi.Services.MinIoService.Interfaces;

public interface IMinioService
{
    Task UploadFileAsync(string bucketName, string objectName, Stream data);
    Task<string> GetFileUrlAsync(string bucketName, string objectName);
}