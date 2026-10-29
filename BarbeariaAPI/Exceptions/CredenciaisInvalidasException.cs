namespace BarbeariaAPI.Exceptions;

/// <summary>Login com e-mail ou senha incorretos (será convertida em 401).</summary>
public sealed class CredenciaisInvalidasException(string mensagem) : Exception(mensagem);