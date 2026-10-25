using BarbeariaAPI.DAOs;
using BarbeariaAPI.DTOs;
using BarbeariaAPI.Exceptions;

namespace BarbeariaAPI.Services;

public class ServicoService(IServicoDao dao) : IServicoService
{
    private const int TamanhoMaximo = 50;

    public async Task<PaginatedResponse<ServicoResponse>> ListarAsync(string? busca, int pagina, int tamanho)
    {
        // O DAO assume valores válidos, então normalizamos aqui.
        pagina = Math.Max(1, pagina);
        tamanho = Math.Clamp(tamanho, 1, TamanhoMaximo);

        var (itens, total) = await dao.ListarAsync(busca, pagina, tamanho);

        return new PaginatedResponse<ServicoResponse>
        {
            Itens = itens.Select(ServicoResponse.FromEntity).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total
        };
    }

    public async Task<ServicoResponse> ObterPorIdAsync(int id)
    {
        var servico = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        return ServicoResponse.FromEntity(servico);
    }

    public async Task<ServicoResponse> CriarAsync(ServicoRequest request)
    {
        Validar(request);

        var criado = await dao.AdicionarAsync(request.ToEntity());
        return ServicoResponse.FromEntity(criado);
    }

    public async Task<ServicoResponse> AtualizarAsync(int id, ServicoRequest request)
    {
        Validar(request);

        // O DAO usa Update, então a existência é checada aqui.
        var servico = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        servico.Nome = request.Nome;
        servico.Descricao = request.Descricao;
        servico.Preco = request.Preco;
        servico.DuracaoMinutos = request.DuracaoMinutos;

        await dao.AtualizarAsync(servico);
        return ServicoResponse.FromEntity(servico);
    }

    public async Task RemoverAsync(int id)
    {
        var servico = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        if (await dao.PossuiAgendamentosAsync(id))
            throw new ConflitoException("Não é possível excluir um serviço que possui agendamentos.");

        await dao.RemoverAsync(servico);
    }

    private static void Validar(ServicoRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            erros.Add("O nome é obrigatório.");
        else if (request.Nome.Length > 100)
            erros.Add("O nome deve ter no máximo 100 caracteres.");

        if (request.Descricao is { Length: > 500 })
            erros.Add("A descrição deve ter no máximo 500 caracteres.");

        if (request.Preco <= 0)
            erros.Add("O preço deve ser maior que zero.");

        if (request.DuracaoMinutos <= 0)
            erros.Add("A duração deve ser maior que zero.");

        if (erros.Count > 0)
            throw new ValidacaoException(erros);
    }
}