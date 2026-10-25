using BarbeariaAPI.DTOs;

namespace BarbeariaAPI.Services;

public interface IServicoService
{
    Task<PaginatedResponse<ServicoResponse>> ListarAsync(string? busca, int pagina, int tamanho);
    Task<ServicoResponse> ObterPorIdAsync(int id);
    Task<ServicoResponse> CriarAsync(ServicoRequest request);
    Task<ServicoResponse> AtualizarAsync(int id, ServicoRequest request);
    Task RemoverAsync(int id);
}