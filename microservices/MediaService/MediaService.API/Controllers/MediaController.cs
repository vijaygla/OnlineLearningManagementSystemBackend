using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using MediaService.Application.Interfaces;
using MediaService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediaService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IMediaRepository _repository;

        public MediaController(IStorageService storageService, IMediaRepository repository)
        {
            _storageService = storageService;
            _repository = repository;
        }

        /// <summary>
        /// Upload a file.
        /// Roles: Admin, Instructor (Course content), Student (Profile pictures)
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Extract User ID from JWT if needed
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? uploadedBy = Guid.TryParse(userIdClaim, out var guid) ? guid : null;

            using var stream = file.OpenReadStream();
            var storagePath = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType);
            var fileUrl = await _storageService.GetFileUrlAsync(storagePath);

            var media = new Media
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                StoragePath = storagePath,
                Url = fileUrl,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = uploadedBy
            };

            await _repository.AddAsync(media);

            return Ok(media);
        }

        /// <summary>
        /// Get media details and a fresh URL.
        /// Roles: Authorized Users
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var media = await _repository.GetByIdAsync(id);
            if (media == null)
                return NotFound();

            // Refresh the URL (in case it's a timed presigned URL from MinIO)
            media.Url = await _storageService.GetFileUrlAsync(media.StoragePath);

            return Ok(media);
        }

        /// <summary>
        /// Delete media.
        /// Roles: Admin (Everything), Instructor/Student (Only their own)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var media = await _repository.GetByIdAsync(id);
            if (media == null)
                return NotFound();

            // Security Check: Only Admin can delete anything. Others delete their own.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && media.UploadedBy.ToString() != userIdClaim)
            {
                return Forbid("You do not have permission to delete this media.");
            }

            await _storageService.DeleteFileAsync(media.StoragePath);
            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}
