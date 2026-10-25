using BarbeariaAPI.DTOs;

namespace BarbeariaAPI.Services;

public interface IAgendamentoService
{
    Task<PaginatedResponse<AgendamentoResponse>> ListarAsync(int? usuarioId, DateTime? data, int pagina, int tamanho);
    Task<AgendamentoResponse> ObterPorIdAsync(int id);
    Task<AgendamentoResponse> CriarAsync(AgendamentoRequest request);
    Task<AgendamentoResponse> AtualizarAsync(int id, AgendamentoRequest request);
    Task RemoverAsync(int id);
}