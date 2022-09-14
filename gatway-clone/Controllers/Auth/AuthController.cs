using Microsoft.AspNetCore.Mvc;


namespace gatway_clone.Controllers.Auth;
[Route("/Auth")]
public abstract class AuthController<TRequest, TResponse> : ServiceController<TRequest, TResponse>
where TRequest : class
where TResponse : class, new()
{
}
