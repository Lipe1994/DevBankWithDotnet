using DevBankWithDotnet.ErrorHandler;
using DevBankWithDotnet.Repositories;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 250;
    o.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers();


builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<NpgsqlContext>();

//police de timeout
builder.Services.AddRequestTimeouts(options => {
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMilliseconds(1200),
        TimeoutStatusCode = 422
    };
});


var app = builder.Build();
app.UseErrorHandler(app.Services.GetService<ILoggerFactory>()!);
app.MapControllers();


app.Run();
