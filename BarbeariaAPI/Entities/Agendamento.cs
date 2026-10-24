namespace BarbeariaAPI.Entities;

public class Agendamento
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } // sempre UTC

    public int ServicoId { get; set; }
    public Servico Servico { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}