using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Domain.Entities;

namespace SearchService.Application.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<CourseSearchIndex>> SearchCoursesAsync(string query);
        Task UpsertCourseIndexAsync(CourseSearchIndex course);
        Task DeleteCourseIndexAsync(string courseId);
        Task<long> GetTotalCountAsync();
    }
}
