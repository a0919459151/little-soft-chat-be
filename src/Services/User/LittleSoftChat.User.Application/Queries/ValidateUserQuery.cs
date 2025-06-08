using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;

namespace LittleSoftChat.User.Application.Queries;

// Query Definition
public record ValidateUserQuery(int UserId) : IRequest<bool>;

// Validator
public class ValidateUserQueryValidator : AbstractValidator<ValidateUserQuery>
{
    public ValidateUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

// Handler
public class ValidateUserQueryHandler : IRequestHandler<ValidateUserQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateUserQueryHandler> _logger;

    public ValidateUserQueryHandler(IUserRepository userRepository, ILogger<ValidateUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            var isValid = user != null && user.IsActive;
            
            _logger.LogInformation("User validation for UserId {UserId}: {IsValid}", 
                request.UserId, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user for UserId: {UserId}", request.UserId);
            return false;
        }
    }
}
