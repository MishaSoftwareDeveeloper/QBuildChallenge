using QBuild.API;
using QBuild.Business.Interfaces;
using QBuild.Business.Services;
using QBuild.DataAccess.Data;
using QBuild.DataAccess.Interfaces;
using QBuild.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = DataDirectoryResolver.Resolve(builder.Configuration, builder.Environment);
var dataStore = BomStartupService.Initialize(dataDirectory);

builder.Services.AddSingleton(dataStore);
builder.Services.AddSingleton<IBomRepository, BomRepository>();
builder.Services.AddSingleton<IPartRepository, PartRepository>();
builder.Services.AddSingleton<IBomService, BomService>();
builder.Services.AddControllers();

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                     ?? ["http://localhost:3000", "http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");
app.MapControllers();
app.Run();
