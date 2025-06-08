using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleSoftChat.User.API.Models;
using LittleSoftChat.User.Application.Commands;
using LittleSoftChat.User.Application.Queries;

namespace LittleSoftChat.User.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        
        return BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.DisplayName);
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        
        return BadRequest(new { message = result.ErrorMessage });
    }
    
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetUserProfileQuery(userId);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new UpdateUserProfileCommand(userId, request.DisplayName, request.Avatar);
        var result = await _mediator.Send(command);
        
        if (result)
        {
            return Ok(new { message = "Profile updated successfully" });
        }
        
        return BadRequest(new { message = "Failed to update profile" });
    }
}
