using BarbeariaAPI.Data;
using BarbeariaAPI.Entities;
using Microsoft.EntityFrameworkCore;

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
        await context.SaveChangesAsync();
        return usuario; // já com o Id gerado pelo banco
    }
}