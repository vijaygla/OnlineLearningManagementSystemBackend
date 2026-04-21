using SharedKernel.Base;

namespace CategoryService.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
