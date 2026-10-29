using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using BarbeariaAPI.DAOs;
using BarbeariaAPI.DTOs;
using BarbeariaAPI.Entities;
using BarbeariaAPI.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BarbeariaAPI.Services;

public class AuthService(IUsuarioDao dao, IConfiguration config) : IAuthService
{
    // Hash válido, usado só para gastar o mesmo tempo quando o e-mail não existe.
    private static readonly string HashFalso = BCrypt.Net.BCrypt.HashPassword("senha-falsa");

    public async Task<AuthResponse> RegistrarAsync(RegistroRequest request)
    {
        Validar(request);

        var email = Normalizar(request.Email);

        if (await dao.EmailExisteAsync(email))
            throw new ConflitoException("E-mail já cadastrado.");

        var criado = await dao.AdicionarAsync(new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Role = Papeis.Cliente // o papel nunca vem do request
        });

        return GerarResposta(criado);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await dao.ObterPorEmailAsync(Normalizar(request.Email));

        // Sempre roda o Verify, mesmo sem usuário, para o tempo de resposta não revelar se o e-mail existe.
        var senhaOk = BCrypt.Net.BCrypt.Verify(request.Senha, usuario?.SenhaHash ?? HashFalso);

        if (usuario is null || !senhaOk)
            throw new CredenciaisInvalidasException("E-mail ou senha inválidos.");

        return GerarResposta(usuario);
    }

    private AuthResponse GerarResposta(Usuario usuario)
    {
        var chave = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var minutos = config.GetValue<int?>("Jwt:ExpiracaoMinutos") ?? 120;
        var expiraEm = DateTime.UtcNow.AddMinutes(minutos);

        // Nomes curtos de claim ("sub", "email", "role"); a leitura é configurada na camada 3.
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("email", usuario.Email),
                new Claim("role", usuario.Role)
            }),
            Expires = expiraEm,
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
                SecurityAlgorithms.HmacSha256)
        };

        return new AuthResponse
        {
            Token = new JsonWebTokenHandler().CreateToken(descriptor),
            ExpiraEm = expiraEm,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role
        };
    }

    private static string Normalizar(string email) => email.Trim().ToLowerInvariant();

    private static void Validar(RegistroRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            erros.Add("O nome é obrigatório.");
        else if (request.Nome.Trim().Length > 100)
            erros.Add("O nome deve ter no máximo 100 caracteres.");

        var email = Normalizar(request.Email ?? string.Empty);
        if (email.Length == 0)
            erros.Add("O e-mail é obrigatório.");
        else if (email.Length > 150)
            erros.Add("O e-mail deve ter no máximo 150 caracteres.");
        else if (!MailAddress.TryCreate(email, out var endereco) || endereco.Address != email)
            erros.Add("O e-mail é inválido.");

        if (string.IsNullOrEmpty(request.Senha) || request.Senha.Length < 6)
            erros.Add("A senha deve ter no mínimo 6 caracteres.");
        else if (Encoding.UTF8.GetByteCount(request.Senha) > 72)
            erros.Add("A senha deve ter no máximo 72 bytes."); // limite do BCrypt

        if (erros.Count > 0)
            throw new ValidacaoException(erros);
    }
}