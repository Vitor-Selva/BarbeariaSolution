using BarbeariaAPI.DAOs;
using BarbeariaAPI.DTOs;
using BarbeariaAPI.Entities;
using BarbeariaAPI.Exceptions;

namespace BarbeariaAPI.Services;

public class AgendamentoService(IAgendamentoDao dao) : IAgendamentoService
{
    private const int TamanhoMaximo = 50;

    public async Task<PaginatedResponse<AgendamentoResponse>> ListarAsync(
        int? usuarioId, DateTime? data, int pagina, int tamanho)
    {
        pagina = Math.Max(1, pagina);
        tamanho = Math.Clamp(tamanho, 1, TamanhoMaximo);

        // O filtro por data cobre o dia inteiro em UTC. O Kind precisa ser Utc para o Npgsql aceitar.
        DateTime? de = null, ate = null;
        if (data is { } d)
        {
            de = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            ate = de.Value.AddDays(1);
        }

        var (itens, total) = await dao.ListarAsync(usuarioId, de, ate, pagina, tamanho);

        return new PaginatedResponse<AgendamentoResponse>
        {
            Itens = itens.Select(AgendamentoResponse.FromEntity).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total
        };
    }

    public async Task<AgendamentoResponse> ObterPorIdAsync(int id)
    {
        var agendamento = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Agendamento não encontrado.");

        return AgendamentoResponse.FromEntity(agendamento);
    }

    public async Task<AgendamentoResponse> CriarAsync(AgendamentoRequest request)
    {
        Validar(request);

        var duracao = await dao.ObterDuracaoServicoAsync(request.ServicoId)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        if (!await dao.UsuarioExisteAsync(request.UsuarioId))
            throw new NaoEncontradoException("Usuário não encontrado.");

        await GarantirSemConflitoAsync(request.DataHora, duracao, ignorarId: null);

        var criado = await dao.AdicionarAsync(request.ToEntity());
        return AgendamentoResponse.FromEntity(criado);
    }

    public async Task<AgendamentoResponse> AtualizarAsync(int id, AgendamentoRequest request)
    {
        Validar(request);

        var existente = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Agendamento não encontrado.");

        var duracao = await dao.ObterDuracaoServicoAsync(request.ServicoId)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        // Ignora o próprio agendamento, senão ele conflitaria consigo mesmo.
        await GarantirSemConflitoAsync(request.DataHora, duracao, ignorarId: id);

        // Entidade limpa, sem Servico/Usuario: o Update não arrasta as navegações do Include.
        // O UsuarioId é mantido; reagendar não troca o cliente.
        await dao.AtualizarAsync(new Agendamento
        {
            Id = id,
            DataHora = request.DataHora,
            ServicoId = request.ServicoId,
            UsuarioId = existente.UsuarioId
        });

        // Busca de novo para o response vir com os nomes atualizados.
        var atualizado = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Agendamento não encontrado.");

        return AgendamentoResponse.FromEntity(atualizado);
    }

    public async Task RemoverAsync(int id)
    {
        _ = await dao.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException("Agendamento não encontrado.");

        await dao.RemoverAsync(id);
    }

    private async Task GarantirSemConflitoAsync(DateTime inicio, int duracaoMinutos, int? ignorarId)
    {
        var fim = inicio.AddMinutes(duracaoMinutos);

        if (await dao.ExisteConflitoAsync(inicio, fim, ignorarId))
            throw new ConflitoException("O horário escolhido conflita com outro agendamento.");
    }

    private static void Validar(AgendamentoRequest request)
    {
        var erros = new List<string>();

        if (request.ServicoId <= 0)
            erros.Add("O serviço é obrigatório.");

        // O Npgsql só aceita Kind=Utc em timestamptz. Rejeitamos em vez de converter em silêncio.
        if (request.DataHora.Kind != DateTimeKind.Utc)
            erros.Add("A data e hora devem ser enviadas em UTC (ex.: 2026-11-03T14:00:00Z).");
        else if (request.DataHora < DateTime.UtcNow)
            erros.Add("A data e hora não podem estar no passado.");

        if (erros.Count > 0)
            throw new ValidacaoException(erros);
    }
}