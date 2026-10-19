using BarbeariaAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Servico> Servicos => Set<Servico>();
}