using FCG02.UsersAPI.Domain.Enums;
using FCG02.UsersAPI.Domain.ValueObjects;

namespace FCG02.UsersAPI.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Email Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = string.Empty;

        public UserRole Role { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Construtor padrão, sem parâmetros, necessário para o Entity Framework Core.
        /// </summary>
        private User() { }

        public User(string name, Email email, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            ArgumentNullException.ThrowIfNull(email, nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash cannot be null or empty.", nameof(passwordHash));

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Atualiza as propriedades do usuário. 
        /// Se algum valor for diferente do atual, a propriedade correspondente será atualizada e a data de atualização será definida como a data e hora atual.
        /// Parâmetros que forem nulos ou vazios não serão atualizados.
        /// </summary>
        /// <param name="name">Novo nome ou null para manter o atual</param>
        /// <param name="email">Novo email ou null para manter o atual</param>
        /// <param name="passwordHash">Nova hash da senha ou null para manter a atual</param>
        /// <param name="role">Novo papel ou null para manter o atual</param>
        public void Update(string? name = null, Email? email = null, string? passwordHash = null, UserRole? role = null)
        {
            bool isUpdated = false;
            if (!string.IsNullOrWhiteSpace(name) && name != Name) { Name = name; isUpdated = true; }
            if (email != null && email != Email) { Email = email; isUpdated = true; }
            if (!string.IsNullOrWhiteSpace(passwordHash) && passwordHash != PasswordHash) { PasswordHash = passwordHash; isUpdated = true; }
            if (role.HasValue && role.Value != Role) { Role = role.Value; isUpdated = true; }
            if (isUpdated) UpdatedAt = DateTime.UtcNow;
        }

        public User UpdateName(string name) { this.Update(name: name); return this; }
        public User UpdateEmail(Email email) { this.Update(email: email); return this; }
        public User UpdatePasswordHash(string passwordHash) { this.Update(passwordHash: passwordHash); return this; }
        public User UpdateRole(UserRole role) { this.Update(role: role); return this; }
    }
}