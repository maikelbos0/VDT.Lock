using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddDbContext<LockContext>((serviceProvider, options) => options
    .UseSqlServer(serviceProvider.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value.ConnectionString)
    .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

var app = builder.Build();

app.MapGet("/test", () => "Hello!");

app.Run();
