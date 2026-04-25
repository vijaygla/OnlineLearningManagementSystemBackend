using System;

namespace MediaService.Domain.Entities
{
    public class Media
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Url { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty; // Path in MinIO
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Guid? UploadedBy { get; set; } // User ID
    }
}
