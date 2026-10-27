using System.ComponentModel.DataAnnotations;

namespace BarbeariaAPI.DTOs;

// Sem campo Role: o papel nunca vem do request (registro público sempre cria "cliente").
public class RegistroRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo {1} caracteres.")]
    [EmailAddress(ErrorMessage = "O e-mail é inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo {1} caracteres.")]
    public string Senha { get; set; } = string.Empty;
}