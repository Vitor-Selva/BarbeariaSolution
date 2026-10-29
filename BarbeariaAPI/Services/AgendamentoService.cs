using BarbeariaAPI.DAOs;
using BarbeariaAPI.DTOs;
using BarbeariaAPI.Entities;
using BarbeariaAPI.Exceptions;

namespace BarbeariaAPI.Services;

public class AgendamentoService(IAgendamentoDao dao) : IAgendamentoService
{
    private const int TamanhoMaximo = 50;

    public async Task<PaginatedResponse<AgendamentoResponse>> ListarAsync(
        int solicitanteId, bool ehAdmin, int? usuarioId, DateTime? data, int pagina, int tamanho)
    {
        pagina = Math.Max(1, pagina);
        tamanho = Math.Clamp(tamanho, 1, TamanhoMaximo);

        // Cliente lista só os próprios (o filtro usuarioId é ignorado); admin pode filtrar por qualquer usuário.
        var filtroUsuario = ehAdmin ? usuarioId : solicitanteId;

        // O filtro por data cobre o dia inteiro em UTC. O Kind precisa ser Utc para o Npgsql aceitar.
        DateTime? de = null, ate = null;
        if (data is { } d)
        {
            de = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            ate = de.Value.AddDays(1);
        }

        var (itens, total) = await dao.ListarAsync(filtroUsuario, de, ate, pagina, tamanho);

        return new PaginatedResponse<AgendamentoResponse>
        {
            Itens = itens.Select(AgendamentoResponse.FromEntity).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total
        };
    }

    public async Task<AgendamentoResponse> ObterPorIdAsync(int solicitanteId, bool ehAdmin, int id) =>
        AgendamentoResponse.FromEntity(await ObterAcessivelAsync(solicitanteId, ehAdmin, id));

    public async Task<AgendamentoResponse> CriarAsync(int solicitanteId, AgendamentoRequest request)
    {
        Validar(request);

        var duracao = await dao.ObterDuracaoServicoAsync(request.ServicoId)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        // Cobre token válido de um usuário que já foi apagado do banco.
        if (!await dao.UsuarioExisteAsync(solicitanteId))
            throw new NaoEncontradoException("Usuário não encontrado.");

        await GarantirSemConflitoAsync(request.DataHora, duracao, ignorarId: null);

        // O dono é sempre quem está logado, inclusive quando é admin.
        var criado = await dao.AdicionarAsync(request.ToEntity(solicitanteId));
        return AgendamentoResponse.FromEntity(criado);
    }

    public async Task<AgendamentoResponse> AtualizarAsync(
        int solicitanteId, bool ehAdmin, int id, AgendamentoRequest request)
    {
        Validar(request);

        var existente = await ObterAcessivelAsync(solicitanteId, ehAdmin, id);

        var duracao = await dao.ObterDuracaoServicoAsync(request.ServicoId)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");

        // Ignora o próprio agendamento, senão ele conflitaria consigo mesmo.
        await GarantirSemConflitoAsync(request.DataHora, duracao, ignorarId: id);

        // Entidade limpa, sem Servico/Usuario: o Update não arrasta as navegações do Include.
        // O UsuarioId original é mantido (inclusive quando o admin reagenda): reagendar não troca o cliente.
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

    public async Task RemoverAsync(int solicitanteId, bool ehAdmin, int id)
    {
        _ = await ObterAcessivelAsync(solicitanteId, ehAdmin, id);
        await dao.RemoverAsync(id);
    }

    // 404 também para agendamento alheio: não revela que o id existe.
    private async Task<Agendamento> ObterAcessivelAsync(int solicitanteId, bool ehAdmin, int id)
    {
        var agendamento = await dao.ObterPorIdAsync(id);

        if (agendamento is null || (!ehAdmin && agendamento.UsuarioId != solicitanteId))
            throw new NaoEncontradoException("Agendamento não encontrado.");

        return agendamento;
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