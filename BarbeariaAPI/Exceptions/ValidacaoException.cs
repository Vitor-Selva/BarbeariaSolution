namespace BarbeariaAPI.Exceptions;

/// <summary>Dados inválidos (será convertida em 400). Carrega todos os erros de uma vez.</summary>
public sealed class ValidacaoException(IReadOnlyList<string> erros)
    : Exception(string.Join(" ", erros))
{
    public IReadOnlyList<string> Erros { get; } = erros;
}