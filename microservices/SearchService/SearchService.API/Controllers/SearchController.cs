using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;

namespace SearchService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search query cannot be empty");

            var results = await _searchService.SearchCoursesAsync(q);
            return Ok(results);
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> Test()
        {
            var count = await _searchService.GetTotalCountAsync();
            return Ok(new { Message = "Search Engine Connected", TotalDocuments = count });
        }

        [HttpPost("sync")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Sync(CourseSearchIndex course)
        {
            await _searchService.UpsertCourseIndexAsync(course);
            return Ok(new { Message = "Sync successful", CourseId = course.Id });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            await _searchService.DeleteCourseIndexAsync(id);
            return NoContent();
        }
    }
}
