using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
    ?? throw new InvalidOperationException();

builder.Services.AddDbContext<LockContext>(options => options
    .UseSqlServer(appSettings.ConnectionString)
    .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddScoped<ISecretHasher, SecretHasher>();
builder.Services.AddScoped<CreateDataStoreRequestHandler>();
builder.Services.AddScoped<LoadDataStoreRequestHandler>();
builder.Services.AddScoped<SaveDataStoreRequestHandler>();

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = appSettings.MaxRequestBodySize);

var app = builder.Build();

app.UseExceptionHandler(applicationBuilder => applicationBuilder.Run(async httpContext 
    => await Results.StatusCode(StatusCodes.Status500InternalServerError).ExecuteAsync(httpContext)));

app.MapPost("/", (
    [FromHeader] string secret,
    [FromServices] CreateDataStoreRequestHandler handler,
    CancellationToken cancellationToken
) => handler.Handle(new CreateDataStoreRequest(Convert.FromBase64String(secret)), cancellationToken));

app.MapGet("/{id}", (
    [FromRoute] Guid id,
    [FromHeader] string secret,
    [FromServices] LoadDataStoreRequestHandler handler,
    CancellationToken cancellationToken
) => handler.Handle(new LoadDataStoreRequest(id, Convert.FromBase64String(secret)), cancellationToken));

app.MapPut("/{id}", (
    [FromRoute] Guid id,
    [FromHeader] string secret,
    [FromBody] byte[] data,
    [FromServices] SaveDataStoreRequestHandler handler
) => handler.Handle(new SaveDataStoreRequest(id, Convert.FromBase64String(secret), data)));

app.Run();
