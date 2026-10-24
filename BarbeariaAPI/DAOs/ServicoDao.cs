using BarbeariaAPI.Data;
using BarbeariaAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaAPI.DAOs;

public class ServicoDao(AppDbContext context) : IServicoDao
{
    public async Task<(IReadOnlyList<Servico> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho)
    {
        var query = context.Servicos.AsNoTracking();

        // Filtro opcional: ILike faz a busca sem diferenciar maiúsculas/minúsculas no PostgreSQL
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var padrao = $"%{busca.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Nome, padrao)
                                  || (s.Descricao != null && EF.Functions.ILike(s.Descricao, padrao)));
        }

        // O total é calculado depois do filtro e antes da paginação
        var total = await query.CountAsync();

        var itens = await query
            .OrderBy(s => s.Nome)
            .ThenBy(s => s.Id) // desempate: garante paginação estável com nomes repetidos
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (itens, total);
    }

    public Task<Servico?> ObterPorIdAsync(int id) =>
        context.Servicos.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Servico> AdicionarAsync(Servico servico)
    {
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();
        return servico; // já com o Id gerado pelo banco
    }

    public async Task AtualizarAsync(Servico servico)
    {
        context.Servicos.Update(servico);
        await context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Servico servico)
    {
        context.Servicos.Remove(servico);
        await context.SaveChangesAsync();
    }

    // Usa a navegação Agendamentos, sem depender do nome da FK nem de um DbSet<Agendamento>
    public Task<bool> PossuiAgendamentosAsync(int servicoId) =>
        context.Servicos.AnyAsync(s => s.Id == servicoId && s.Agendamentos.Any());
}