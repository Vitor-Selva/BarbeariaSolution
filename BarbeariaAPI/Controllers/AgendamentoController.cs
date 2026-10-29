using BarbeariaAPI.DTOs;
using BarbeariaAPI.Entities;
using BarbeariaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Controllers;

[ApiController]
[Authorize] // qualquer usuário logado; a regra "cliente só vê os próprios" fica no service
[Route("api/[controller]")]
public class AgendamentosController(IAgendamentoService service) : ControllerBase
{
    // Claims com nome curto ("sub"), pois o MapInboundClaims está desligado no Program.cs.
    private int UsuarioLogadoId => int.Parse(User.FindFirst("sub")!.Value);
    private bool EhAdmin => User.IsInRole(Papeis.Admin);

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AgendamentoResponse>>> Listar(
        [FromQuery] int? usuarioId, [FromQuery] DateTime? data, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10) =>
        Ok(await service.ListarAsync(UsuarioLogadoId, EhAdmin, usuarioId, data, pagina, tamanho));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgendamentoResponse>> ObterPorId(int id) =>
        Ok(await service.ObterPorIdAsync(UsuarioLogadoId, EhAdmin, id));

    [HttpPost]
    public async Task<ActionResult<AgendamentoResponse>> Criar(AgendamentoRequest request)
    {
        var criado = await service.CriarAsync(UsuarioLogadoId, request);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AgendamentoResponse>> Atualizar(int id, AgendamentoRequest request) =>
        Ok(await service.AtualizarAsync(UsuarioLogadoId, EhAdmin, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await service.RemoverAsync(UsuarioLogadoId, EhAdmin, id);
        return NoContent();
    }
}