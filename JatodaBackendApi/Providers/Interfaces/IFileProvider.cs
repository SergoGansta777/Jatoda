namespace JatodaBackendApi.Providers.Interfaces;

public interface IFileProvider
{
    Task<string> UploadFileAsync(IFormFile? file);
    Task<string> GetFileUrlAsync(string fileName);
    Task<Stream?> GetFileAsync(string fileName);

}