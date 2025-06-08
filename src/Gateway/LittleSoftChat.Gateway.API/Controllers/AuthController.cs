using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LittleSoftChat.Gateway.API.Models;
using LittleSoftChat.Gateway.API.Services;

namespace LittleSoftChat.Gateway.API.Controllers;

/// <summary>
/// 用戶認證相關 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 用戶登入
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <returns>登入結果</returns>
    /// <response code="200">登入成功</response>
    /// <response code="400">登入失敗</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        
        return BadRequest(new { message = result.ErrorMessage });
    }

    /// <summary>
    /// 用戶註冊
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <returns>註冊結果</returns>
    /// <response code="200">註冊成功</response>
    /// <response code="400">註冊失敗</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        
        return BadRequest(new { message = result.ErrorMessage });
    }
    
    /// <summary>
    /// 取得用戶個人資料
    /// </summary>
    /// <returns>用戶個人資料</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">用戶不存在</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfile), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _userService.GetUserProfileAsync(userId);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// 更新用戶個人資料
    /// </summary>
    /// <param name="request">更新請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _userService.UpdateUserProfileAsync(userId, request);
        
        if (result)
        {
            return Ok(new { message = "Profile updated successfully" });
        }
        
        return BadRequest(new { message = "Failed to update profile" });
    }

    /// <summary>
    /// 搜尋用戶
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <returns>搜尋結果</returns>
    /// <response code="200">搜尋成功</response>
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(List<UserProfile>), 200)]
    public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
    {
        var result = await _userService.SearchUsersAsync(searchTerm);
        return Ok(result);
    }
}
