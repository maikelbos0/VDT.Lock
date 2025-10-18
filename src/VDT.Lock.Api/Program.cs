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
using System.Threading.RateLimiting;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
    ?? throw new InvalidOperationException();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddDbContext<LockContext>(options => options
    .UseSqlServer(appSettings.ConnectionString)
    .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
builder.Services.AddHttpLogging(options => { });
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["Secret"].ToString(),
            factory: _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }
        )
    );
    options.AddPolicy("CreateDataStore", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(10)
            }
        )
    );
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy
    => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddScoped<ISecretHasher, SecretHasher>();
builder.Services.AddScoped<CreateDataStoreRequestHandler>();
builder.Services.AddScoped<LoadDataStoreRequestHandler>();
builder.Services.AddScoped<SaveDataStoreRequestHandler>();

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = appSettings.MaxRequestBodySize);

var app = builder.Build();

app.UseHttpLogging();
app.UseRateLimiter();
app.UseExceptionHandler(applicationBuilder => applicationBuilder.Run(async httpContext
    => await Results.StatusCode(StatusCodes.Status500InternalServerError).ExecuteAsync(httpContext)));
app.UseCors();

app.MapPost("/", (
    [FromHeader] string secret,
    [FromServices] CreateDataStoreRequestHandler handler,
    CancellationToken cancellationToken
) => handler.Handle(new CreateDataStoreRequest(Convert.FromBase64String(secret)), cancellationToken)).RequireRateLimiting("CreateDataStore");

app.MapGet("/{id}", (
    [FromRoute] Guid id,
    [FromHeader] string secret,
    [FromServices] LoadDataStoreRequestHandler handler,
    CancellationToken cancellationToken
) => handler.Handle(new LoadDataStoreRequest(id, Convert.FromBase64String(secret)), cancellationToken));

app.MapPut("/{id}", (
    [FromRoute] Guid id,
    [FromHeader] string secret,
    HttpRequest request,
    [FromServices] SaveDataStoreRequestHandler handler
) => {
    return handler.Handle(new SaveDataStoreRequest(id, Convert.FromBase64String(secret), request.Body, request.ContentLength));
});

app.Run();
