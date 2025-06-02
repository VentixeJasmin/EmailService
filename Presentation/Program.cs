//using Azure.Communication.Email;
//using Azure.Messaging.ServiceBus;
//using Presentation.Interfaces;
//using Presentation.ServiceBus;
//using Presentation.Services;

//Console.WriteLine("=== EMAIL SERVICE STARTING ===");

//var builder = WebApplication.CreateBuilder(args);
//Console.WriteLine("=== BUILDER CREATED ===");

//Console.WriteLine($"ServiceBus: {builder.Configuration.GetConnectionString("ServiceBus")}");
//Console.WriteLine($"ACS: {builder.Configuration.GetConnectionString("ACS")}");
//Console.WriteLine($"Redis: {builder.Configuration.GetConnectionString("Redis")}");
//Console.WriteLine($"ACS:SenderAddress: {builder.Configuration["ACS:SenderAddress"]}");
//Console.WriteLine("=== EMAIL SERVICE CONFIG CHECK ===");

//builder.Services.AddControllers();
//builder.Services.AddOpenApi();

////Got help with the ServiceBus configuring by Claude AI
//builder.Services.AddSingleton<ServiceBusClient>(provider =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("ServiceBus");

//    Console.WriteLine($"ServiceBus connection: {connectionString != null}");

//    return new ServiceBusClient(connectionString);
//});

//builder.Services.AddHostedService<AccountCreatedMessageHandler>();

//Console.WriteLine("=== BACKGROUND SERVICE REGISTERED ===");

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//});
//builder.Services.AddSingleton(x => new EmailClient(builder.Configuration["ConnectionStrings:ACS"])); 
//builder.Services.AddTransient<IVerificationService, VerificationService>();

//Console.WriteLine("=== ABOUT TO BUILD APP ===");

//var app = builder.Build();

//Console.WriteLine("=== APP BUILT, ABOUT TO RUN ===");


//app.MapOpenApi();

//app.UseHttpsRedirection();
//app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//var logger = app.Services.GetRequiredService<ILogger<Program>>();
//logger.LogInformation("Email service is starting...");

//app.Run();


Console.WriteLine("=== MINIMAL EMAIL SERVICE TEST ===");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "EmailService is running!");

Console.WriteLine("=== STARTING MINIMAL APP ===");
app.Run();


