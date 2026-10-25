using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DTOs;

public class AgendamentoResponse
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public int ServicoId { get; set; }
    public string ServicoNome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;

    // Exige Servico e Usuario carregados (o DAO faz o Include).
    public static AgendamentoResponse FromEntity(Agendamento agendamento) => new()
    {
        Id = agendamento.Id,
        DataHora = agendamento.DataHora,
        ServicoId = agendamento.ServicoId,
        ServicoNome = agendamento.Servico.Nome,
        UsuarioId = agendamento.UsuarioId,
        UsuarioNome = agendamento.Usuario.Nome
    };
}