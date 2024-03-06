using DevBankWithDotnet.ErrorHandler;
using DevBankWithDotnet.Repositories;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 160;
    o.AddServerHeader = false;
});

builder.Services.AddSingleton<NpgsqlContext>();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ClienteRepository>();

//police de timeout
builder.Services.AddRequestTimeouts(options => {
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMilliseconds(2000),
        TimeoutStatusCode = 422
    };
});

var app = builder.Build();
app.UseErrorHandler(app.Services.GetService<ILoggerFactory>()!);
app.MapControllers();

app.Run();
