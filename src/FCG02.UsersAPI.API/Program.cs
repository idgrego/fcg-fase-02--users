using FCG02.UsersAPI.Infrastructure.Extensions;
using FCG02.UsersAPI.Application.Extensions;
using Scalar.AspNetCore;
using FCG02.UsersAPI.API.Endpoints;
using FCG02.UsersAPI.Application.DTOs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Adiciona os serviços de infraestrutura, incluindo o DbContext e os repositórios
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Configuração do JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings == null) throw new InvalidOperationException("JWT settings are not configured properly.");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    /*
    O que é: Define qual esquema será usado para reconhecer/identificar quem é o usuário a cada requisição que chega.
    O que acontece ao definir como JwtBearerDefaults.AuthenticationScheme:
    O ASP.NET Core usará o manipulador de JWT Bearer para inspecionar a requisição (lendo o cabeçalho Authorization ou o Cookie jwt_token), validar a assinatura do token e preencher a propriedade HttpContext.User com as claims do usuário.
    Sem isso: Se você não definir um esquema padrão de autenticação, o ASP.NET Core não saberá qual middleware chamar para ler o token, e o HttpContext.User ficará anônimo.
    */
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    /*
    O que é: Define o que o ASP.NET Core deve fazer quando um usuário não autenticado (anônimo) tenta acessar uma rota protegida por [Authorize]. A isso chamamos de "Challenge" (Desafio).
    O que acontece ao definir como JwtBearerDefaults.AuthenticationScheme:
    Quando um usuário anônimo tentar acessar uma rota restrita, o framework invocará o manipulador de JWT Bearer, que responderá automaticamente com o código de status HTTP 401 Unauthorized (e o cabeçalho WWW-Authenticate: Bearer).
    Comparação prática:
    Em APIs / JWT: O Challenge padrão retorna HTTP 401 Unauthorized.
    Em Aplicações MVC puras com Cookie: O Challenge padrão geralmente redireciona o usuário para a tela de login (/Account/Login?ReturnUrl=...).
    */
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Exige (ou não) que a comunicação e a descoberta de metadados do provedor de autenticação aconteçam exclusivamente via HTTPS.
    // options.RequireHttpsMetadata = false; // em desenvolvimento

    // Instrui o ASP.NET Core a salvar o token JWT que veio na requisição dentro do contexto da requisição HTTP (HttpContext)
    // Se em alguma Controller ou Service você precisar acessar o token em texto puro 
    // (por exemplo, para repassar a chamada para outra API interna), 
    // você consegue recuperar com: 
    // var token = await HttpContext.GetTokenAsync("access_token");
    // options.SaveToken = true;

    // Este objeto define a "lista de verificação" (checklist) que o ASP.NET executará 
    // em todo e qualquer token recebido antes de permitir o acesso às suas Controllers.
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        /* -- Resumo do fluxo de validação -- 
        
        Requisição HTTP com Token
         │
         ▼
        1. A chave bate com a nossa SecretKey? (ValidateIssuerSigningKey / IssuerSigningKey)
         │ Sim
         ▼
        2. Foi o nosso sistema que emitiu? (ValidateIssuer / ValidIssuer)
         │ Sim
         ▼
        3. Foi emitido para esta aplicação? (ValidateAudience / ValidAudience)
         │ Sim
         ▼
        4. O token ainda está no prazo de validade sem tolerância extra? (ValidateLifetime / ClockSkew)
         │ Sim
         ▼
        ✅ USUÁRIO AUTENTICADO! (Preenche HttpContext.User com as Claims)
        
        */

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretKey),

        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        // Verifica se o token ainda está dentro do seu prazo de validade 
        // (comparando as claims nbf [Not Before] e exp [Expiration] com o horário atual 
        // DateTime.UtcNow). Se o token expirou, retorna 401 Unauthorized.
        ValidateLifetime = true,
        // Por padrão, o .NET adiciona uma tolerância de 5 minutos (ClockSkew = TimeSpan.FromMinutes(5)) para compensar possíveis desincronizações de relógio entre diferentes servidores.
        // Definindo TimeSpan.Zero, você remove essa tolerância. Se o token expirar às 14:00:00, às 14:00:01 ele já é considerado inválido instantaneamente.
        ClockSkew = TimeSpan.Zero // Remove o tempo de tolerância padrão de 5 minutos
    };
});


builder.Services.AddAuthorization();

// 3. Documentação OpenAPI nativa do .NET (Scalar)
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Pipeline HTTP
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("FIAP Cloud Games - UsersAPI");
        options.WithTheme(ScalarTheme.Moon);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 5. Mapeamento Modular dos Route Groups
app.MapAuthEndpoints();
app.MapUserEndpoints();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Health",
    service = "UsersAPI",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.Run();
