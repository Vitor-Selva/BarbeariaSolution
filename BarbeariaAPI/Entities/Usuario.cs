namespace BarbeariaAPI.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty; // hash BCrypt
    public string Role { get; set; } = "cliente";         // "admin" ou "cliente"

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}