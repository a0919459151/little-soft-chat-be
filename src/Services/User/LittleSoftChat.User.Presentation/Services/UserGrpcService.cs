using Grpc.Core;
using MediatR;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.User.Application.Queries;
using LittleSoftChat.User.Application.Commands;

namespace LittleSoftChat.User.Presentation.Services;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(IMediator mediator, ILogger<UserGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetUserByIdQuery(request.UserId);
            var user = await _mediator.Send(query);
            
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetUsersByIdsQuery(request.UserIds.ToList());
            var users = await _mediator.Send(query);
            
            var response = new GetUsersResponse();
            response.Users.AddRange(users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Avatar = u.Avatar,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }));
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
    {
        try
        {
            var query = new ValidateUserQuery(request.UserId);
            var isValid = await _mediator.Send(query);
            
            return new ValidateUserResponse
            {
                IsValid = isValid,
                ErrorMessage = isValid ? "" : "User not found or inactive"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user {UserId}", request.UserId);
            return new ValidateUserResponse
            {
                IsValid = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request, ServerCallContext context)
    {
        try
        {
            var query = new SearchUsersQuery(request.Keyword);
            var users = await _mediator.Send(query);
            
            var response = new SearchUsersResponse();
            response.Users.AddRange(users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Avatar = u.Avatar,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }));
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with keyword {Keyword}", request.Keyword);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var command = new UpdateUserProfileCommand(request.UserId, request.DisplayName, request.Avatar);
            var result = await _mediator.Send(command);
            
            return new UpdateUserResponse
            {
                Success = result,
                ErrorMessage = result ? "" : "Failed to update user profile"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", request.UserId);
            return new UpdateUserResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await _mediator.Send(command);
            
            var response = new LoginResponse
            {
                IsSuccess = result.IsSuccess,
                Token = result.Token,
                ErrorMessage = result.ErrorMessage
            };

            if (result.IsSuccess && result.User != null)
            {
                response.User = new UserResponse
                {
                    Id = result.User.Id,
                    Username = result.User.Username,
                    DisplayName = result.User.DisplayName,
                    Avatar = result.User.Avatar,
                    IsActive = result.User.IsActive,
                    CreatedAt = result.User.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return new LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            var command = new RegisterCommand(request.Username, request.Email, request.Password, request.DisplayName);
            var result = await _mediator.Send(command);
            
            var response = new RegisterResponse
            {
                IsSuccess = result.IsSuccess,
                Token = result.Token,
                ErrorMessage = result.ErrorMessage
            };

            if (result.IsSuccess && result.User != null)
            {
                response.User = new UserResponse
                {
                    Id = result.User.Id,
                    Username = result.User.Username,
                    DisplayName = result.User.DisplayName,
                    Avatar = result.User.Avatar,
                    IsActive = result.User.IsActive,
                    CreatedAt = result.User.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return new RegisterResponse
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during registration"
            };
        }
    }
}
