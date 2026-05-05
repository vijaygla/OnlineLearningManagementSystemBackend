using System.Threading.Tasks;
using MassTransit;
using SearchService.Application.Interfaces;
using Shared.Contracts.Events;

namespace SearchService.Application.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly ISearchService _searchService;

        public UserDeletedConsumer(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            await _searchService.DeleteUserIndexAsync(context.Message.UserId.ToString());
        }
    }
}
