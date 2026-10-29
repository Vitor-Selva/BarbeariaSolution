using System.ComponentModel.DataAnnotations;
using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DTOs;

public class AgendamentoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "O serviço é obrigatório.")]
    public int ServicoId { get; set; }

    // Deve vir em UTC (ex.: 2026-11-03T14:00:00Z). O service valida o Kind.
    public DateTime DataHora { get; set; }

    // O dono vem do token, nunca do corpo da requisição.
    public Agendamento ToEntity(int usuarioId) => new() { DataHora = DataHora, ServicoId = ServicoId, UsuarioId = usuarioId };
}