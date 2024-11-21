using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Services zone - begin
builder.Services.AddScoped<IGenresRepository, GenresRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]!).AllowAnyMethod().AllowAnyHeader();
    });

    options.AddPolicy("free",
        configuration => { configuration.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddOutputCache();
// Services zone - end

#region swagger setup

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
#endregion

var app = builder.Build();
// Middlewares zone - begin
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseOutputCache();


app.MapGet("/", () => { return "salam"; });

// مثال استفاده از یک میدولور برای یک روت خاص
// app.MapGet("/genres", [EnableCors(policyName: "free")]() =>
app.MapGroup("/genres").MapGenres();

// Middlewares zone - end

app.Run();