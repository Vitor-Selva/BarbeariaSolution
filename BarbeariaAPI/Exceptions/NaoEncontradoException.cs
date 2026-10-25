namespace BarbeariaAPI.Exceptions;

/// <summary>Recurso inexistente (será convertida em 404).</summary>
public sealed class NaoEncontradoException(string mensagem) : Exception(mensagem);