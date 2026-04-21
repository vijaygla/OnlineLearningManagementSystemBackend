using System;

namespace SharedKernel.Base;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
