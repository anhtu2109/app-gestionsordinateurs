var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// HttpClientFactory gère les connexions HTTP réutilisables vers l'API métier.
builder.Services.AddHttpClient("Utilisateurs", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiUrl"]!);
    c.Timeout = TimeSpan.FromSeconds(8);
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5020").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
