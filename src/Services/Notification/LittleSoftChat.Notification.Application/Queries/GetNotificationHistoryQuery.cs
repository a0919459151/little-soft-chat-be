using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Notification.Application.Queries;

// Query Definition
public record GetNotificationHistoryQuery(
    int UserId,
    int Page = 1,
    int Size = 20
) : IRequest<PagedResult<NotificationDto>>;

// Validator
public class GetNotificationHistoryQueryValidator : AbstractValidator<GetNotificationHistoryQuery>
{
    public GetNotificationHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .WithMessage("Size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Size cannot exceed 100");
    }
}

// Handler
public class GetNotificationHistoryQueryHandler : IRequestHandler<GetNotificationHistoryQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationHistoryRepository _historyRepository;
    private readonly ILogger<GetNotificationHistoryQueryHandler> _logger;

    public GetNotificationHistoryQueryHandler(
        INotificationHistoryRepository historyRepository,
        ILogger<GetNotificationHistoryQueryHandler> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notifications = await _historyRepository.GetByUserIdAsync(request.UserId, request.Page, request.Size);
            
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            }).ToList();

            // Get total count for pagination (simplified approach)
            var totalCount = notifications.Count; // 簡化實現，實際應該有總數查詢

            _logger.LogInformation("Retrieved {Count} notifications for user {UserId}, page {Page}", 
                notificationDtos.Count, request.UserId, request.Page);

            return new PagedResult<NotificationDto>(notificationDtos, request.Page, request.Size, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification history for user {UserId}", request.UserId);
            return new PagedResult<NotificationDto>();
        }
    }
}
