using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DAOs;

public interface IAgendamentoDao
{
    Task<(IReadOnlyList<Agendamento> Itens, int Total)> ListarAsync(
    int? usuarioId, DateTime? de, DateTime? ate, int pagina, int tamanho);
    Task<Agendamento?> ObterPorIdAsync(int id);
    Task<Agendamento> AdicionarAsync(Agendamento agendamento);
    Task AtualizarAsync(Agendamento agendamento);
    Task RemoverAsync(int id);
    Task<int?> ObterDuracaoServicoAsync(int servicoId);
    Task<bool> UsuarioExisteAsync(int usuarioId);
    Task<bool> ExisteConflitoAsync(DateTime inicio, DateTime fim, int? ignorarId);
}