using BarbeariaAPI.DTOs;
using BarbeariaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Controllers;

// Sem [Authorize]: registro e login são públicos. Rotas: api/auth/registro e api/auth/login.
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistroRequest request) =>
        StatusCode(StatusCodes.Status201Created, await service.RegistrarAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
        Ok(await service.LoginAsync(request));
}