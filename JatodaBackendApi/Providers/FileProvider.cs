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
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            await _minioService.UploadFileAsync("mybucket", file.FileName, stream);
        }

        _logger.LogInformation("File uploaded successfully");
        return "File uploaded successfully";
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        var url = await _minioService.GetFileUrlAsync("my-bucket", fileName);
        _logger.LogInformation("File URL retrieved successfully");
        return url;
    }

    public async Task<Stream?> GetFileAsync(string fileName)
    {
        var fileStream = await _minioService.GetObjectAsync("my-bucketl", fileName);
        _logger.LogInformation("File retrieved successfull");
        return fileStream;
    }
}