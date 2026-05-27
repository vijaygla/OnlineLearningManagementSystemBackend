using SharedKernel.Base;

namespace ContentService.Domain.Entities;

/// <summary>
/// Represents a lesson or module within a course.
/// </summary>
public class Lesson : BaseAuditableEntity
{
    public Guid CourseId { get; set; } // Links to the Course Service
    public Guid SectionId { get; set; } // Links to the Section
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty; // URL to Video/PDF
    public string ContentType { get; set; } = "Video"; // Video, PDF, Text
    public int Order { get; set; } // Sequence of the lesson in the section
    
    // Navigation property
    [System.Text.Json.Serialization.JsonIgnore]
    public Section? Section { get; set; }
}
