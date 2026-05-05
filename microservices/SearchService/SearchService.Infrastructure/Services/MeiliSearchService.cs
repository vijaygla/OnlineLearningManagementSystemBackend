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
        private readonly Meilisearch.Index _courseIndex;
        private readonly Meilisearch.Index _userIndex;
        private const string CourseIndexName = "courses";
        private const string UserIndexName = "users";

        public MeiliSearchService(IConfiguration configuration)
        {
            var url = Environment.GetEnvironmentVariable("MEILI_URL") ?? "http://localhost:7700";
            var masterKey = Environment.GetEnvironmentVariable("MEILI_MASTER_KEY") ?? "masterKey123";
            
            _client = new MeilisearchClient(url, masterKey);
            _courseIndex = _client.Index(CourseIndexName);
            _userIndex = _client.Index(UserIndexName);
        }

        public async Task<IEnumerable<CourseSearchIndex>> SearchCoursesAsync(string query)
        {
            try
            {
                var results = await _courseIndex.SearchAsync<CourseSearchIndex>(query);
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
                var task = await _courseIndex.AddDocumentsAsync(new[] { course });
                await _client.WaitForTaskAsync(task.TaskUid);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
                await _client.CreateIndexAsync(CourseIndexName, "id");
                var task = await _courseIndex.AddDocumentsAsync(new[] { course });
                await _client.WaitForTaskAsync(task.TaskUid);
            }
        }

        public async Task DeleteCourseIndexAsync(string courseId)
        {
            try
            {
                await _courseIndex.DeleteOneDocumentAsync(courseId);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
            }
        }

        public async Task<IEnumerable<UserSearchIndex>> SearchUsersAsync(string query)
        {
            try
            {
                var results = await _userIndex.SearchAsync<UserSearchIndex>(query);
                return results.Hits;
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
                return Enumerable.Empty<UserSearchIndex>();
            }
        }

        public async Task UpsertUserIndexAsync(UserSearchIndex user)
        {
            try
            {
                var task = await _userIndex.AddDocumentsAsync(new[] { user });
                await _client.WaitForTaskAsync(task.TaskUid);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
                await _client.CreateIndexAsync(UserIndexName, "id");
                var task = await _userIndex.AddDocumentsAsync(new[] { user });
                await _client.WaitForTaskAsync(task.TaskUid);
            }
        }

        public async Task DeleteUserIndexAsync(string userId)
        {
            try
            {
                await _userIndex.DeleteOneDocumentAsync(userId);
            }
            catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
            {
            }
        }

        public async Task<long> GetTotalCountAsync()
        {
            try
            {
                var courseStats = await _courseIndex.GetStatsAsync();
                var userStats = await _userIndex.GetStatsAsync();
                return courseStats.NumberOfDocuments + userStats.NumberOfDocuments;
            }
            catch
            {
                return 0;
            }
        }
    }
}
