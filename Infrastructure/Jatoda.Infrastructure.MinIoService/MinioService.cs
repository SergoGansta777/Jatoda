using Jatoda.Domain.Core.Options;
using Jatoda.Infrastructure.MinIoService.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Jatoda.Infrastructure.MinIoService;

public class MinioService : IMinioService
{
    private const int HourInSeconds = 3600;
    private readonly IMinioClient _minioClient;
    private readonly IOptions<MinioOptions> _minioOptions;

    public MinioService(IOptions<MinioOptions> minioOptions, IMinioClient minioClient)
    {
        _minioOptions = minioOptions;
        _minioClient = minioClient;
    }

    public async Task UploadFileAsync(string bucketName, string objectName, Stream data)
    {
        var found = await _minioClient!.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName))
                .ConfigureAwait(false);
        }

        await _minioClient.PutObjectAsync(new PutObjectArgs().WithBucket(bucketName).WithFileName(objectName)
            .WithStreamData(data));
    }

    public async Task<string> GetFileUrlAsync(string bucketName, string objectName)
    {
        return await _minioClient!.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(bucketName)
            .WithObject(objectName).WithExpiry(HourInSeconds));
    }

    public async Task<Stream?> GetObjectAsync(string bucketName, string objectName)
    {
        try
        {
            var memoryStream = new MemoryStream();

            await _minioClient!.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(s => s.CopyTo(memoryStream)));

            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving the file: {ex.Message}", ex);
        }
    }
}