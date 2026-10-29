using System.ComponentModel.DataAnnotations;

namespace GestionOrdinateurs.Shared;

public enum RoleEnum { Gestionnaire, Admin }

public class User
{
    public int Id { get; set; }

    [Required]
    public string Login { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }
}

public record LoginRequest(string Login, RoleEnum Role);
public record TokenResponse(string Token, DateTime ExpiresAt);
