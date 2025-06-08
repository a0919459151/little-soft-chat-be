namespace LittleSoftChat.Shared.Domain.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message, Exception innerException) 
        : base(message, innerException) { }
        
    public ExternalServiceException(string message) 
        : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
