using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Queries;

// Query Definition
public record SearchUsersQuery(string Keyword) : IRequest<List<UserDto>>;

// Validator
public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .NotEmpty()
            .WithMessage("Search keyword is required")
            .MinimumLength(2)
            .WithMessage("Search keyword must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Search keyword cannot exceed 100 characters");
    }
}

// Handler
public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SearchUsersQueryHandler> _logger;

    public SearchUsersQueryHandler(IUserRepository userRepository, ILogger<SearchUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.SearchUsersAsync(request.Keyword);
            
            var activeUsers = users.Where(u => u.IsActive).ToList();
            
            _logger.LogInformation("Found {Count} active users for keyword: {Keyword}", 
                activeUsers.Count, request.Keyword);

            return activeUsers.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with keyword: {Keyword}", request.Keyword);
            throw;
        }
    }
}
