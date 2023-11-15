using JatodaBackendApi.Services.MinIoService.Interfaces;
using Minio;
using Minio.DataModel.Args;

namespace JatodaBackendApi.Services.MinIoService;

public class MinioService : IMinioService
{
    private readonly MinioClient? _minioClient;

    public MinioService(string endpoint, string accessKey, string secretKey)
    {
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
}