using System.Text.Json.Serialization;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Evenements;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
// Partie 3 : Swagger envoie le JWT dans l'en-tête Authorization.
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Coller uniquement le token JWT."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidIssuer = "AuthentificationApi",
        ValidateAudience = true,
        ValidAudience = "GestionOrdinateurs",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5000", "http://localhost:5020").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHttpClient("Devises", c =>
{
    c.BaseAddress = new Uri("https://api.frankfurter.dev/");
    c.Timeout = TimeSpan.FromSeconds(8);
});

// Le chemin absolu évite de créer une seconde base selon le dossier du terminal.
builder.Services.AddDbContextFactory<ParcInformatiqueContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ParcInformatique")
        ?? "Data Source=" + Path.Combine(builder.Environment.ContentRootPath, "parcinfo.db")));
builder.Services.AddSingleton(new JournalService(Path.Combine(builder.Environment.ContentRootPath, "Logs")));
builder.Services.AddSingleton<JournalOrdinateurs>();
builder.Services.AddSingleton<ReparationService>();
builder.Services.AddSingleton<StockService>();
builder.Services.AddSingleton<IOrdinateurService>(services =>
{
    var service = new OrdinateurService(services.GetRequiredService<IDbContextFactory<ParcInformatiqueContext>>());
    var journal = services.GetRequiredService<JournalOrdinateurs>();
    // Les abonnements sont installés une seule fois.
    service.OrdinateurAjoute += journal.AfficherMessageAjout;
    service.OrdinateurSupprime += journal.AfficherMessageSuppression;
    service.OrdinateurMisAJour += journal.AfficherMessageMiseAJour;
    return service;
});

var app = builder.Build();

// Partie 1 : Migrate conserve les données du TP4 et ajoute Users et Vendu.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();
    db.Database.Migrate();
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new GestionOrdinateurs.Shared.User { Login = "user1", Role = GestionOrdinateurs.Shared.RoleEnum.Admin },
            new GestionOrdinateurs.Shared.User { Login = "user2", Role = GestionOrdinateurs.Shared.RoleEnum.Gestionnaire },
            new GestionOrdinateurs.Shared.User { Login = "user3", Role = GestionOrdinateurs.Shared.RoleEnum.Gestionnaire });
        db.SaveChanges();
    }
}
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<GestionOrdinateurs.API.Middleware.RequestLoggerMiddleware>();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
