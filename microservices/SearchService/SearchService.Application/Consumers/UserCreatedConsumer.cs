using System.Threading.Tasks;
using MassTransit;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;
using Shared.Contracts.Events;

namespace SearchService.Application.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ISearchService _searchService;

        public UserCreatedConsumer(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var user = new UserSearchIndex
            {
                Id = context.Message.UserId.ToString(),
                Name = context.Message.FullName,
                Email = context.Message.Email,
                Role = "Student", // Default role when created via normal registration
                CreatedAt = context.Message.CreatedAt
            };

            await _searchService.UpsertUserIndexAsync(user);
        }
    }
}
