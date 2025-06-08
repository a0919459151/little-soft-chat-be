using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Chat.Application.Queries;

public record GetMessagesQuery(int UserId, int FriendId, int Page = 1, int Size = 20) : IRequest<PagedResult<MessageDto>>;

public class GetMessagesQueryValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.FriendId)
            .GreaterThan(0)
            .WithMessage("FriendId must be greater than 0");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be between 1 and 100");

        RuleFor(x => x)
            .Must(query => query.UserId != query.FriendId)
            .WithMessage("UserId and FriendId cannot be the same");
    }
}

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<GetMessagesQueryHandler> _logger;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        ILogger<GetMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _messageRepository.GetMessagesAsync(request.UserId, request.FriendId, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for users {UserId} and {FriendId}", 
                request.UserId, request.FriendId);
            return new PagedResult<MessageDto>(new List<MessageDto>(), request.Page, request.Size, 0);
        }
    }
}
