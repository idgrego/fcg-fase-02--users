using System.Security.Claims;
using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Interfaces;

namespace FCG02.UsersAPI.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization(); // Adiciona a exigência de autorização para todos os endpoints do grupo

        DefineMeRoute(group);

        return app;
    }

    private static void DefineMeRoute(IEndpointRouteBuilder group)
    {
        group.MapGet("/me", async (ClaimsPrincipal claims, IUserService userService) =>
        {
            var userIdClaim = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userService.GetByIdAsync(userId);
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Retrieves the currently authenticated user's information.")
        .WithDescription("This endpoint returns the details of the user associated with the provided JWT token.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}