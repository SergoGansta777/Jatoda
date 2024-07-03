using JatodaBackendApi.Options;
using JatodaBackendApi.Services.MinIoService.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace JatodaBackendApi.Services.MinIoService;

public class MinioService : IMinioService
{
    private const int HourInSeconds = 3600;
    private readonly MinioClient? _minioClient;

    public MinioService(IOptions<MinioOptions> minioOptions)
    {
        var endpoint = minioOptions.Value.Endpoint;
        var accessKey = minioOptions.Value.AccessKey;
        var secretKey = minioOptions.Value.SecretKey;

        _minioClient = (MinioClient?) new MinioClient().WithEndpoint(endpoint).WithCredentials(accessKey, secretKey)
            .Build();
    }

    public async Task UploadFileAsync(string bucketName, string objectName, Stream data)
    {
        // Check if bucket exists, if not, create it
        var found = await _minioClient!.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName))
                .ConfigureAwait(false);

        // Upload the file
        await _minioClient.PutObjectAsync(new PutObjectArgs().WithBucket(bucketName).WithFileName(objectName)
            .WithStreamData(data));
    }

    public async Task<string> GetFileUrlAsync(string bucketName, string objectName)
    {
        // Generate a presigned URL that is valid for 1 hour
        return await _minioClient!.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(bucketName)
            .WithObject(objectName).WithExpiry(3600));
    }

    public async Task<Stream?> GetObjectAsync(string bucketName, string objectName)
    {
        try
        {
            // Create a new MemoryStream that will hold the file data
            var memoryStream = new MemoryStream();

            // Retrieve the file data from MinIO and write it to the MemoryStream
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(s => s.CopyTo(memoryStream)));

            // Reset the MemoryStream position to the start
            memoryStream.Position = 0;

            // Return the MemoryStream that now contains the file data
            return memoryStream;
        }
        catch (Exception ex)
        {
            // Handle any errors that may occur
            throw new Exception($"An error occurred while retrieving the file: {ex.Message}", ex);
        }
    }
}