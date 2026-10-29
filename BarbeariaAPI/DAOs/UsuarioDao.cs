using BarbeariaAPI.Data;
using BarbeariaAPI.Entities;
using Microsoft.EntityFrameworkCore;
using BarbeariaAPI.Exceptions;
using Npgsql;

namespace BarbeariaAPI.DAOs;

// O service envia o e-mail já normalizado (trim + minúsculas); aqui a comparação é direta.
public class UsuarioDao(AppDbContext context) : IUsuarioDao
{
    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> EmailExisteAsync(string email) =>
        context.Usuarios.AnyAsync(u => u.Email == email);

    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        context.Usuarios.Add(usuario);
        try
        {
            await context.SaveChangesAsync();
        }
        // Corrida no registro: dois pedidos passaram pela checagem e o índice único barrou o segundo.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflitoException("E-mail já cadastrado.");
        }
        return usuario; // já com o Id gerado pelo banco
    }
}