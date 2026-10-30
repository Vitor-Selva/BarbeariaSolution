using System.Text;
using BarbeariaAPI.DAOs;
using BarbeariaAPI.Data;
using BarbeariaAPI.Services;
using BarbeariaAPI.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Falha na inicialização, com mensagem clara, se a chave faltar ou for curta (HS256 exige 32+).
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException(
        "Jwt:Key ausente ou com menos de 32 caracteres. Defina nos user-secrets ou na variável Jwt__Key.");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IServicoDao, ServicoDao>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IAgendamentoDao, AgendamentoDao>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IUsuarioDao, UsuarioDao>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarbeariaAPI",
        Version = "v1",
        Description = "API REST para gerenciar uma barbearia: serviços, agendamentos e autenticação JWT. " +
                      "AVISO: este é um projeto escolar, desenvolvido apenas para fins de estudo (curso de TADS). " +
                      "Não utilize dados reais nem este sistema em produção. " +
                      "Para testar as rotas protegidas: faça login em POST /api/auth/login, copie o token e clique em Authorize."
    });

    // Scheme "bearer" faz o Swagger UI acrescentar o prefixo "Bearer " sozinho; no botão se cola só o token.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Informe apenas o token JWT, sem o prefixo 'Bearer'."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantém os nomes curtos do token ("sub", "role"), sem o mapeamento automático para URIs longas.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role", // usado por [Authorize(Roles = ...)]
            NameClaimType = "sub"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

// Em produção o Render termina o HTTPS e a API só escuta HTTP; o redirecionamento ficaria sem porta HTTPS.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

// SHA do commit em execução. O Render define RENDER_GIT_COMMIT no deploy; localmente a variável não existe.
var commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT");
if (string.IsNullOrWhiteSpace(commit))
    commit = "desconhecido";

app.MapGet("/", () => new { Mensagem = "API Funcionando" });

app.MapGet("/health", () => Results.Ok("Healthy"));

// Público (sem [Authorize]) e responde application/json.
app.MapGet("/api/versao", () => Results.Ok(new { commit }));

app.Run();