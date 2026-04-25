using System;
using System.IO;
using System.Threading.Tasks;
using MediaService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace MediaService.Infrastructure.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public MinioStorageService(IConfiguration configuration)
        {
            var endpoint = Environment.GetEnvironmentVariable("MinIO__Endpoint") ?? configuration["MinIO:Endpoint"] ?? "localhost:9000";
            var accessKey = Environment.GetEnvironmentVariable("MinIO__AccessKey") ?? configuration["MinIO:AccessKey"] ?? "minioadmin";
            var secretKey = Environment.GetEnvironmentVariable("MinIO__SecretKey") ?? configuration["MinIO:SecretKey"] ?? "minioadmin";
            _bucketName = Environment.GetEnvironmentVariable("MinIO__BucketName") ?? configuration["MinIO:BucketName"] ?? "olms-media";

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false) // Set to true if using HTTPS
                .Build();
            
            // Ensure bucket exists (in a real app, this might be done during startup)
            EnsureBucketExistsAsync().Wait();
        }

        private async Task EnsureBucketExistsAsync()
        {
            var args = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(args);
            if (!found)
            {
                var makeArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(makeArgs);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Create a unique name to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            return uniqueFileName;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            // In MinIO, we can get a presigned URL that lasts for a certain time
            var getArgs = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithExpiry(3600); // 1 hour

            return await _minioClient.PresignedGetObjectAsync(getArgs);
        }
    }
}
