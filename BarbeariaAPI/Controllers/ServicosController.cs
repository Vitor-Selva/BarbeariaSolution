using BarbeariaAPI.DTOs;
using BarbeariaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicosController(IServicoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ServicoResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        => Ok(await service.ListarAsync(busca, pagina, tamanho));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServicoResponse>> Obter(int id)
        => Ok(await service.ObterPorIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<ServicoResponse>> Criar(ServicoRequest request)
    {
        var criado = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServicoResponse>> Atualizar(int id, ServicoRequest request)
        => Ok(await service.AtualizarAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}