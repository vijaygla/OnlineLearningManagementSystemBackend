using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meilisearch;
using Microsoft.Extensions.Configuration;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Services
{
    public class MeiliSearchService : ISearchService
    {
        private readonly MeilisearchClient _client;
        private readonly Meilisearch.Index _index;
        private const string IndexName = "courses";

        public MeiliSearchService(IConfiguration configuration)
        {
            var url = Environment.GetEnvironmentVariable("MEILI_URL") ?? "http://localhost:7700";
            var masterKey = Environment.GetEnvironmentVariable("MEILI_MASTER_KEY") ?? "masterKey123";
            
            _client = new MeilisearchClient(url, masterKey);
            _index = _client.Index(IndexName);
        }

        public async Task<IEnumerable<CourseSearchIndex>> SearchCoursesAsync(string query)
        {
            try
            {
                var results = await _index.SearchAsync<CourseSearchIndex>(query);
                return results.Hits;
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
                return Enumerable.Empty<CourseSearchIndex>();
            }
        }

        public async Task UpsertCourseIndexAsync(CourseSearchIndex course)
        {
            try
            {
                var task = await _index.AddDocumentsAsync(new[] { course });
                // We wait for the task to finish so it's searchable immediately in dev environment
                await _client.WaitForTaskAsync(task.TaskUid);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
                await _client.CreateIndexAsync(IndexName, "id");
                var task = await _index.AddDocumentsAsync(new[] { course });
                await _client.WaitForTaskAsync(task.TaskUid);
            }
        }

        public async Task DeleteCourseIndexAsync(string courseId)
        {
            try
            {
                await _index.DeleteOneDocumentAsync(courseId);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
            }
        }

        public async Task<long> GetTotalCountAsync()
        {
            try
            {
                var stats = await _index.GetStatsAsync();
                return stats.NumberOfDocuments;
            }
            catch
            {
                return 0;
            }
        }
    }
}
