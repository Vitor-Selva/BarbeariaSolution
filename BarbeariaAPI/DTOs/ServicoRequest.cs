using System.ComponentModel.DataAnnotations;
using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DTOs;

public class ServicoRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true,
        ErrorMessage = "O preço deve ser maior que zero e ter no máximo 8 dígitos inteiros.")]
    public decimal Preco { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser maior que zero.")]
    public int DuracaoMinutos { get; set; }

    public Servico ToEntity() => new()
    {
        Nome = Nome,
        Descricao = Descricao,
        Preco = Preco,
        DuracaoMinutos = DuracaoMinutos
    };
}