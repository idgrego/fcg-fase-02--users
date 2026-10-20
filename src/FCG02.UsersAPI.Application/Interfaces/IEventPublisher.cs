namespace FCG02.UsersAPI.Application.Interfaces;

public interface IEventPublisher
{

    /*
    
    O símbolo @ antes de event em C# é usado para permitir que uma palavra reservada da linguagem (keyword) seja utilizada como nome de uma variável ou parâmetro.
    No C#, a palavra event é reservada para declarar eventos em classes (usada no padrão de publish/subscribe nativo da linguagem).
    
    */


    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}