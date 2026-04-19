using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaasStarter.Api.Middleware;
using SaasStarter.Application;
using SaasStarter.Infra;
using SaasStarter.Infra.Persistence;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Application & Infrastructure ────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── OpenAPI ──────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        // TODO: atualize o título e descrição com o nome do seu SaaS
        document.Info.Title = "SaasStarter API";
        document.Info.Description = "Backend template com autenticação JWT, pagamentos Stripe e gestão de planos.";
        return Task.CompletedTask;
    });
});

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret não configurado em appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("CorsPolicy", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    }
    else
    {
        var frontendUrl = builder.Configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl não configurado.");

        // TODO: adicione aqui outros domínios permitidos em produção
        options.AddPolicy("CorsPolicy", policy =>
            policy.WithOrigins(frontendUrl)
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    }
});

// ─── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration["ConnectionStrings:DefaultConnection"]!,
        name: "postgresql",
        tags: ["db", "ready"]);

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware ───────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "SaasStarter API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ─── Auto-migrate on startup ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
