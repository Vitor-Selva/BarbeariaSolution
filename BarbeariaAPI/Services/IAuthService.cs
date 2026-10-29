using BarbeariaAPI.DTOs;

namespace BarbeariaAPI.Services;

public interface IAuthService
{
    Task<AuthResponse> RegistrarAsync(RegistroRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}