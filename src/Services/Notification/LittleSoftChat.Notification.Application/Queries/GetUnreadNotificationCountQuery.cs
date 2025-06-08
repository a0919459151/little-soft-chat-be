using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Domain.Repositories;

namespace LittleSoftChat.Notification.Application.Queries;

// Query Definition
public record GetUnreadNotificationCountQuery(
    int UserId
) : IRequest<int>;

// Validator
public class GetUnreadNotificationCountQueryValidator : AbstractValidator<GetUnreadNotificationCountQuery>
{
    public GetUnreadNotificationCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

// Handler
public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly INotificationHistoryRepository _historyRepository;
    private readonly ILogger<GetUnreadNotificationCountQueryHandler> _logger;

    public GetUnreadNotificationCountQueryHandler(
        INotificationHistoryRepository historyRepository,
        ILogger<GetUnreadNotificationCountQueryHandler> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var count = await _historyRepository.GetUnreadCountAsync(request.UserId);
            
            _logger.LogInformation("Retrieved unread notification count {Count} for user {UserId}", 
                count, request.UserId);
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread notification count for user {UserId}", request.UserId);
            return 0;
        }
    }
}
