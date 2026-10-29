using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaAPI.Exceptions;

/// <summary>Converte as exceções de negócio em respostas ProblemDetails (404, 409, 400, 401).</summary>
public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int Status, string Titulo)? mapeado = exception switch
        {
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            ConflitoException => (StatusCodes.Status409Conflict, "Conflito."),
            ValidacaoException => (StatusCodes.Status400BadRequest, "Dados inválidos."),
            CredenciaisInvalidasException => (StatusCodes.Status401Unauthorized, "Não autorizado."),
            _ => null
        };

        // Exceção inesperada: não tratamos aqui, o framework responde 500.
        if (mapeado is not { } m) return false;

        var problem = new ProblemDetails { Status = m.Status, Title = m.Titulo, Detail = exception.Message };

        if (exception is ValidacaoException validacao)
            problem.Extensions["erros"] = validacao.Erros;

        httpContext.Response.StatusCode = m.Status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}