using BarbeariaAPI.DTOs;
using BarbeariaAPI.Entities;
using BarbeariaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicosController(IServicoService service) : ControllerBase
{
    // GET públicos: o catálogo pode ser consultado sem login.
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<ServicoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ServicoResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        => Ok(await service.ListarAsync(busca, pagina, tamanho));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicoResponse>> Obter(int id)
        => Ok(await service.ObterPorIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Papeis.Admin)]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServicoResponse>> Criar(ServicoRequest request)
    {
        var criado = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Papeis.Admin)]
    [ProducesResponseType<ServicoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicoResponse>> Atualizar(int id, ServicoRequest request)
        => Ok(await service.AtualizarAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Papeis.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(int id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}