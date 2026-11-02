using System.Text;
using System.Text.Json;
using FCG02.UsersAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FCG02.UsersAPI.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public RabbitMqEventPublisher(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqEventPublisher> logger)
    {
        this._logger = logger;
        this._settings = options.Value;
        this._factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            AutomaticRecoveryEnabled = true
        };
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsOpen)
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null && _connection.IsOpen)
                return _connection;

            _logger.LogDebug("🔌 Conectando ao RabbitMQ em {Host}:{Port}...", _settings.HostName, _settings.Port);
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var connection = await GetConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Garante que a Exchange existe (Idempotente e durável)
            await channel.ExchangeDeclareAsync(
                exchange: _settings.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var eventName = typeof(TEvent).Name;
            var routingKey = ResolveRoutingKey(eventName);

            var payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                // a instrução Converters acima garante que as enumerações são passadas como texto e não como número.
                //(Dessa forma o payload trafega como "role": "Gamer", respeitando o padrão EDA de eventos autodescritivos e legíveis).
            });
            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Type = eventName,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _settings.Exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("🚀 [RabbitMQ] Evento '{EventName}' publicado com sucesso na exchange '{Exchange}' (RoutingKey: '{RoutingKey}')",
                eventName, _settings.Exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [RabbitMQ] Falha ao publicar evento '{EventName}'. Verifique a conectividade com o broker.", typeof(TEvent).Name);
            // Em cenários de produção, aplicar Outbox Pattern para garantir entrega garantida (At-Least-Once)
            throw;
        }
    }

    private static string ResolveRoutingKey(string eventName) => eventName switch
    {
        "UserCreatedEvent" => "user.created",
        _ => "user.generic"
    };

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        _connectionLock.Dispose();
        GC.SuppressFinalize(this);
    }
}