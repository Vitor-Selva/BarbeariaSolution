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
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistroRequest request) =>
        StatusCode(StatusCodes.Status201Created, await service.RegistrarAsync(request));

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
        Ok(await service.LoginAsync(request));
}