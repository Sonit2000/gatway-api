
namespace gatway_clone;

public static class Extension
{
public static string? GetIP(this HttpContext context)
   => context.Request.Headers.ContainsKey("X-Forwarded-For") ?
       context.Request.Headers["X-Forwarded-For"].ToString() :
       context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
}

