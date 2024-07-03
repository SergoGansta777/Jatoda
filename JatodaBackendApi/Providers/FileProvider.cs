using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.MinIoService.Interfaces;

namespace JatodaBackendApi.Providers;

public class FileProvider : IFileProvider
{
    private readonly ILogger<FileProvider> _logger;
    private readonly IMinioService _minioService;

    public FileProvider(IMinioService minioService, ILogger<FileProvider> logger)
    {
        _minioService = minioService;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile? file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            await _minioService.UploadFileAsync("mybucket", file.FileName, stream);

            _logger.LogInformation("File uploaded successfully");
            return "File uploaded successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the file");
            return "An error occurred while uploading the file";
        }
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        try
        {
            var url = await _minioService.GetFileUrlAsync("my-bucket", fileName);
            _logger.LogInformation("File URL retrieved successfully");
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the file URL");
            return "An error occurred while retrieving the file URL";
        }
    }

    public async Task<Stream?> GetFileAsync(string fileName)
    {
        try
        {
            var fileStream = await _minioService.GetObjectAsync("my-bucketl", fileName);
            _logger.LogInformation("File retrieved successfull");
            return fileStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the file");
            return null;
        }
    }
}