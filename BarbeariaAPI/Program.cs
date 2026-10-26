using BarbeariaAPI.DAOs;
using BarbeariaAPI.Data;
using BarbeariaAPI.Services;
using BarbeariaAPI.Exceptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IServicoDao, ServicoDao>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IAgendamentoDao, AgendamentoDao>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => new { Mensagem = "API Funcionando" });

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();