using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DAOs;

public interface IServicoDao
{
    Task<(IReadOnlyList<Servico> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho);
    Task<Servico?> ObterPorIdAsync(int id);
    Task<Servico> AdicionarAsync(Servico servico);
    Task AtualizarAsync(Servico servico);
    Task RemoverAsync(Servico servico);
    Task<bool> PossuiAgendamentosAsync(int servicoId);
}