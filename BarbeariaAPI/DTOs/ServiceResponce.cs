using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DTOs;

public class ServicoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int DuracaoMinutos { get; set; }

    public static ServicoResponse FromEntity(Servico servico) => new()
    {
        Id = servico.Id,
        Nome = servico.Nome,
        Descricao = servico.Descricao,
        Preco = servico.Preco,
        DuracaoMinutos = servico.DuracaoMinutos
    };
}