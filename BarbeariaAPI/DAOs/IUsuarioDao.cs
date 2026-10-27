using BarbeariaAPI.Entities;

namespace BarbeariaAPI.DAOs;

public interface IUsuarioDao
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email);
    Task<Usuario> AdicionarAsync(Usuario usuario);
}