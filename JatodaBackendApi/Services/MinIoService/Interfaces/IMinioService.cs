using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JatodaBackendApi.Services.MinIoService.Interfaces
{
    public interface IMinioService
    {
        Task UploadFileAsync(string bucketName, string objectName, Stream data);
        Task<string> GetFileUrlAsync(string bucketName, string objectName);
    }
}