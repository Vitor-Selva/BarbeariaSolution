namespace BarbeariaAPI.Entities;

/// <summary>Papéis do sistema. Constantes evitam erro de digitação em [Authorize(Roles = ...)].</summary>
public static class Papeis
{
    public const string Admin = "admin";
    public const string Cliente = "cliente";
}