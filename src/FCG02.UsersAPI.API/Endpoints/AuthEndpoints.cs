using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Interfaces;

namespace FCG02.UsersAPI.API.Endpoints;

public static class AuthEndPoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        DefineRegisterRoute(group);
        DefineLoginRoute(group);

        return app;
    }

    private static void DefineRegisterRoute(IEndpointRouteBuilder group)
    {
        group.MapPost("/register", async (RegisterUserRequest request, IUserService userService) =>
        {
            try
            {
                var response = await userService.RegisterAsync(request);
                return Results.Created($"/api/v1/users/{response.Id}", response);
            }
            catch (System.ArgumentException ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.Message
                });
            }
        })
        .WithName("RegisterUser")
        .WithSummary("Registers a new user in the system.")
        .WithDescription("This endpoint allows you to register a new user and publish the UserCreatedEvent")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }

    private static void DefineLoginRoute(IEndpointRouteBuilder group)
    {
        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (System.ArgumentException ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                //return Results.Unauthorized();
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.Message
                });
            }
        })
        .WithName("LoginUser")
        .WithSummary("Authenticates a user and returns a JWT token.")
        .WithDescription("This endpoint allows you to authenticate a user and receive a JWT token for subsequent requests.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}