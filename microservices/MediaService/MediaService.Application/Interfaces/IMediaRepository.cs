using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaService.Domain.Entities;

namespace MediaService.Application.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media?> GetByIdAsync(Guid id);
        Task<IEnumerable<Media>> GetAllAsync();
        Task AddAsync(Media media);
        Task DeleteAsync(Guid id);
    }
}
