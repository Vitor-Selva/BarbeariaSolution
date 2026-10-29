using BarbeariaAPI.DTOs;

namespace BarbeariaAPI.Services;

// solicitanteId e ehAdmin vêm do token (o controller extrai); o service aplica a regra de acesso.
public interface IAgendamentoService
{
    Task<PaginatedResponse<AgendamentoResponse>> ListarAsync(
        int solicitanteId, bool ehAdmin, int? usuarioId, DateTime? data, int pagina, int tamanho);
    Task<AgendamentoResponse> ObterPorIdAsync(int solicitanteId, bool ehAdmin, int id);
    Task<AgendamentoResponse> CriarAsync(int solicitanteId, AgendamentoRequest request);
    Task<AgendamentoResponse> AtualizarAsync(int solicitanteId, bool ehAdmin, int id, AgendamentoRequest request);
    Task RemoverAsync(int solicitanteId, bool ehAdmin, int id);
}