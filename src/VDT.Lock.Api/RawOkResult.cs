using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace VDT.Lock.Api;

public sealed record RawOkResult : IResult, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<byte[]>, IEndpointMetadataProvider {
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(byte[]), ["application/octet-stream"]));
    }

    public byte[] Value { get; }

    object? IValueHttpResult.Value => Value;

    public int StatusCode => StatusCodes.Status200OK;

    int? IStatusCodeHttpResult.StatusCode => StatusCodes.Status200OK;


    public RawOkResult(byte[] value) {
        Value = value;
    }

    public async Task ExecuteAsync(HttpContext httpContext) {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.Body.WriteAsync(Value);
    }
}
