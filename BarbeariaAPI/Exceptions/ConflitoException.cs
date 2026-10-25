namespace BarbeariaAPI.Exceptions;

/// <summary>Operação viola uma regra de negócio (será convertida em 409).</summary>
public sealed class ConflitoException(string mensagem) : Exception(mensagem);