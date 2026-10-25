using BarbeariaAPI.Data;
using BarbeariaAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaAPI.DAOs;

public class AgendamentoDao(AppDbContext context) : IAgendamentoDao
{
    public async Task<(IReadOnlyList<Agendamento> Itens, int Total)> ListarAsync(
        int? usuarioId, DateTime? de, DateTime? ate, int pagina, int tamanho)
    {
        var query = context.Agendamentos.AsNoTracking();

        if (usuarioId is not null)
            query = query.Where(a => a.UsuarioId == usuarioId);

        // Intervalo [de, ate): o início é inclusivo e o fim é exclusivo.
        if (de is not null)
            query = query.Where(a => a.DataHora >= de);

        if (ate is not null)
            query = query.Where(a => a.DataHora < ate);

        var total = await query.CountAsync();
        // ... Include, OrderBy, Skip, Take e retorno continuam como estavam

        var itens = await query
            .Include(a => a.Servico)
            .Include(a => a.Usuario)
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id) // desempate: paginação estável
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (itens, total);
    }

    public Task<Agendamento?> ObterPorIdAsync(int id) =>
        context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Servico)
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Agendamento> AdicionarAsync(Agendamento agendamento)
    {
        context.Agendamentos.Add(agendamento);
        await context.SaveChangesAsync();

        // Carrega Servico e Usuario para o response ter os nomes.
        await context.Entry(agendamento).Reference(a => a.Servico).LoadAsync();
        await context.Entry(agendamento).Reference(a => a.Usuario).LoadAsync();
        return agendamento;
    }

    public async Task AtualizarAsync(Agendamento agendamento)
    {
        context.Agendamentos.Update(agendamento);
        await context.SaveChangesAsync();
    }

    // Por id: Remove no objeto do Include arrastaria Servico e Usuario junto.
    public Task RemoverAsync(int id) =>
        context.Agendamentos.Where(a => a.Id == id).ExecuteDeleteAsync();

    // null = serviço não existe; senão devolve a duração para calcular o fim.
    public Task<int?> ObterDuracaoServicoAsync(int servicoId) =>
        context.Servicos
            .Where(s => s.Id == servicoId)
            .Select(s => (int?)s.DuracaoMinutos)
            .FirstOrDefaultAsync();

    public Task<bool> UsuarioExisteAsync(int usuarioId) =>
        context.Usuarios.AnyAsync(u => u.Id == usuarioId);

    // Há conflito se inicioA < fimB && inicioB < fimA. Encostar (fim == início) não conflita.
    public Task<bool> ExisteConflitoAsync(DateTime inicio, DateTime fim, int? ignorarId) =>
        context.Agendamentos.AnyAsync(a =>
            (ignorarId == null || a.Id != ignorarId)
            && a.DataHora < fim
            && a.DataHora.AddMinutes(a.Servico.DuracaoMinutos) > inicio);
}