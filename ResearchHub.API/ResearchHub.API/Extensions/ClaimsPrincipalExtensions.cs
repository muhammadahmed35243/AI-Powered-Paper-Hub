using System.Security.Claims;

namespace ResearchHub.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
