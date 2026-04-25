using MassTransit;
using Shared.Contracts.Events;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(IUserProfileRepository repository, ILogger<UserCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received UserCreatedEvent for UserId: {UserId}", message.UserId);

        var exists = await _repository.ExistsAsync(message.UserId);
        if (exists)
        {
            _logger.LogInformation("UserProfile for UserId: {UserId} already exists.", message.UserId);
            return;
        }

        var profile = new UserProfile
        {
            UserId = message.UserId,
            FullName = message.FullName,
            CreatedAt = message.CreatedAt == default ? DateTime.UtcNow : message.CreatedAt,
            LastUpdatedAt = DateTime.UtcNow,
            Preference = new UserPreference
            {
                UserId = message.UserId
            }
        };

        await _repository.AddAsync(profile);
        _logger.LogInformation("UserProfile created for UserId: {UserId}", message.UserId);
    }
}
