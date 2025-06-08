using Grpc.Core;
using MediatR;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.User.Application.Queries;

namespace LittleSoftChat.User.API.Services;

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
}
