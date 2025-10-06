using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddDbContext<LockContext>((serviceProvider, options) => options
    .UseSqlServer(serviceProvider.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value.ConnectionString)
    .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddScoped<ISecretHasher, SecretHasher>();
builder.Services.AddScoped<CreateDataStoreRequestHandler>();

var app = builder.Build();

app.MapPost("/",  ([FromBody] CreateDataStoreRequest request, [FromServices] CreateDataStoreRequestHandler handler) => handler.Handle(request));

app.Run();
