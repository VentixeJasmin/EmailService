using Azure.Communication.Email;
using Presentation.Interfaces;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//InMemoryDatabase eller något 
builder.Services.AddSingleton(x => new EmailClient(builder.Configuration["ACS:ConnectionString"])); 
builder.Services.AddTransient<IVerificationService, VerificationService>();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
