using System;

namespace SharedKernel.Base;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
