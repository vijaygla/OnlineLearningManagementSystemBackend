using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaService.Application.Interfaces;
using MediaService.Domain.Entities;
using MediaService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly MediaDbContext _context;

        public MediaRepository(MediaDbContext context)
        {
            _context = context;
        }

        public async Task<Media?> GetByIdAsync(Guid id)
        {
            return await _context.Media.FindAsync(id);
        }

        public async Task<IEnumerable<Media>> GetAllAsync()
        {
            return await _context.Media.ToListAsync();
        }

        public async Task AddAsync(Media media)
        {
            await _context.Media.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var media = await _context.Media.FindAsync(id);
            if (media != null)
            {
                _context.Media.Remove(media);
                await _context.SaveChangesAsync();
            }
        }
    }
}
