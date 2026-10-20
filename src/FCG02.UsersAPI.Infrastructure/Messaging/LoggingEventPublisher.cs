using System.Text.Json;
using FCG02.UsersAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG02.UsersAPI.Infrastructure.Messaging;

public class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;
    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger;
    }



    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        var eventName = typeof(TEvent).Name;
        var payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true });

        _logger.LogInformation("📢 [EVENTO PUBLICADO PROVISORIAMENTE]: {EventName}\n{Payload}", eventName, payload);

        return Task.CompletedTask;
    }
}