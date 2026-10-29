var builder = WebApplication.CreateBuilder(args);
// Partie 5 : routes métier explicites, aucune route attrape-tout vers les API.
// Auth et Users sont aussi exposés pour que la connexion passe par la Gateway.
// Cela concilie la question 14 avec l'architecture et la question 16 de l'énoncé.
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5020").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors();
app.MapReverseProxy();
app.Run();
