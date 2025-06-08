using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LittleSoftChat.Shared.Infrastructure.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName} with {@Request}", requestName, request);

        try
        {
            var response = await next();
            _logger.LogInformation("Completed {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 基本驗證邏輯 (不使用 FluentValidation)
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 可以加入簡單的屬性驗證
        ValidateRequest(request);

        return await next();
    }

    private static void ValidateRequest(TRequest request)
    {
        // 簡單驗證邏輯
        var properties = typeof(TRequest).GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            
            // 檢查必填字串
            if (property.PropertyType == typeof(string) && value is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                if (property.Name.Contains("Id") == false) // ID 可能為 0
                {
                    throw new ArgumentException($"{property.Name} cannot be empty");
                }
            }
            
            // 檢查必填數字
            if (property.PropertyType == typeof(int) && value is int intValue && intValue <= 0)
            {
                if (property.Name.Contains("Id") || property.Name.Contains("UserId"))
                {
                    throw new ArgumentException($"{property.Name} must be greater than 0");
                }
            }
        }
    }
}

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500) // 超過 500ms 記錄警告
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} ms) {@Request}",
                requestName, elapsedMilliseconds, request);
        }

        return response;
    }
}
