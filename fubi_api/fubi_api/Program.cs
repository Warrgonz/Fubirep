using fubi_api.Models;
using fubi_api.Utils.S3;
using fubi_api.Utils.Smtp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IMessage, Message>();
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings"));

// S3

builder.Services.AddSingleton<IBucket, Bucket>();

// Agregar otros servicios a la aplicación
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
