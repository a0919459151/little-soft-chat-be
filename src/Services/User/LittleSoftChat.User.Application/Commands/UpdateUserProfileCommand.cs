using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;

namespace LittleSoftChat.User.Application.Commands;

public record UpdateUserProfileCommand(int UserId, string DisplayName, string Avatar) : IRequest<bool>;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required")
            .MinimumLength(2)
            .WithMessage("DisplayName must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("DisplayName cannot exceed 100 characters");

        RuleFor(x => x.Avatar)
            .MaximumLength(500)
            .WithMessage("Avatar URL cannot exceed 500 characters");
    }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            user.DisplayName = request.DisplayName;
            user.Avatar = request.Avatar;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);
            
            if (result)
            {
                _logger.LogInformation("User profile updated for user {UserId}", request.UserId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
            return false;
        }
    }
}
