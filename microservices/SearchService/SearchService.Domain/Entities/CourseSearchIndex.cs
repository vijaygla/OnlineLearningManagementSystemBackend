using System;
using System.Text.Json.Serialization;

namespace SearchService.Domain.Entities
{
    public class CourseSearchIndex
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("instructorName")]
        public string InstructorName { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
