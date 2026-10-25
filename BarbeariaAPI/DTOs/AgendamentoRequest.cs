using System.ComponentModel.DataAnnotations;
using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DTOs;

public class AgendamentoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "O serviço é obrigatório.")]
    public int ServicoId { get; set; }

    // Só é usado na criação (o service responde 404 se não existir). No PUT é ignorado.
    public int UsuarioId { get; set; }

    // Deve vir em UTC (ex.: 2026-11-03T14:00:00Z). O service valida o Kind.
    public DateTime DataHora { get; set; }

    public Agendamento ToEntity() => new() { DataHora = DataHora, ServicoId = ServicoId, UsuarioId = UsuarioId };
}