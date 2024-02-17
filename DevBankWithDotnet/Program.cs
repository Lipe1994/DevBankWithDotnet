using DevBankWithDotnet.Repositories;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 300;
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
        Timeout = TimeSpan.FromMilliseconds(800),
        TimeoutStatusCode = 422
    };
});


var app = builder.Build();

app.MapControllers();


app.Run();
