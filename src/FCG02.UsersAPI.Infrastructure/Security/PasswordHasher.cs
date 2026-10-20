using FCG02.UsersAPI.Application.Interfaces;

namespace FCG02.UsersAPI.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    // Custo de processamento recomendado
    // (Work Factor 11 a 12 equilibra segurança
    //  com tempo de resposta < 100ms)
    private const int WorkFactor = 11;

    public string HashPassword(string plainTextPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainTextPassword, nameof(plainTextPassword));
        return BCrypt.Net.BCrypt.EnhancedHashPassword(plainTextPassword, WorkFactor);


        /*
        
        Por que BCrypt ao invés de SHA256 / MD5?
        SHA-256 é uma função criptográfica rápida, projetada para integridade de arquivos. Por ser rápida, uma GPU moderna consegue testar bilhões de combinações por segundo em um ataque de força bruta.
        O BCrypt é intencionalmente lento e adaptativo (usa Work Factor / Cost factor ajustável), embutindo um Salt criptográfico aleatório único por senha, neutralizando ataques com tabelas Rainbow e GPU cracking.
        
        */

        // Implementação de hashing de senha usando SHA256
        /* using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainTextPassword);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash); */
    }

    public bool VerifyPassword(string plainTextPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        return BCrypt.Net.BCrypt.EnhancedVerify(plainTextPassword, passwordHash);
    }
}