using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Events;
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.Interfaces;
using FCG02.UsersAPI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FCG02.UsersAPI.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserService>? _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEventPublisher eventPublisher,
        ILogger<UserService>? logger = null)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is not null ? UserResponse.FromEntity(user) : null;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        _logger?.LogInformation("👤 [UsersAPI] Usuário solicitou o auto registro: '{Name}' ({Email}).", request.Name, request.Email);

        // cria e valida o Value Object Email
        var email = new Email(request.Email);

        // Verifica se o usuário já existe
        var existingUser = await _userRepository.EmailExistsAsync(email.Value, cancellationToken);
        if (existingUser)
            throw new InvalidOperationException("User with this email already exists.");

        // Cria o hash da senha
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Cria a entidade User
        var user = new User(request.Name, email, passwordHash, request.Role);

        // Salva o usuário no repositório
        await _userRepository.AddAsync(user, cancellationToken);

        _logger?.LogInformation("✅ [UsersAPI] Auto registro efetuado com sucesso para o usuário '{Name}' (Id: {UserId}). Tabela(s) modificada(s): Users.", user.Name, user.Id);

        // Publica o evento de usuário registrado
        var userCreatedEvent = new UserCreatedEvent
        (
            EventId: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email.Value,
            Role: user.Role
        );
        await _eventPublisher.PublishAsync<UserCreatedEvent>(userCreatedEvent, cancellationToken);

        _logger?.LogInformation("🚀 [UsersAPI] Objeto 'UserCreatedEvent' foi enviado para a fila e deverá ser capturado pelo microsserviço de notificações para envio do e-mail de boas-vindas.");

        // Retorna a resposta do usuário criado
        return UserResponse.FromEntity(user);
    }
}