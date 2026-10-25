using BarbeariaAPI.DTOs;
using BarbeariaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController(IAgendamentoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AgendamentoResponse>>> Listar(
        [FromQuery] int? usuarioId, [FromQuery] DateTime? data, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10) =>
        Ok(await service.ListarAsync(usuarioId, data, pagina, tamanho));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgendamentoResponse>> ObterPorId(int id) =>
        Ok(await service.ObterPorIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<AgendamentoResponse>> Criar(AgendamentoRequest request)
    {
        var criado = await service.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AgendamentoResponse>> Atualizar(int id, AgendamentoRequest request) =>
        Ok(await service.AtualizarAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}