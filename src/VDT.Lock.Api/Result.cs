using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace VDT.Lock.Api;

public sealed record Result(int StatusCode, byte[]? Data) : IResult, IStatusCodeHttpResult {
    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    public Result(int statusCode) : this(statusCode, null) { }

    public async Task ExecuteAsync(HttpContext httpContext) {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        if (Data != null) {
            await httpContext.Response.Body.WriteAsync(Data);
        }
    }
}
