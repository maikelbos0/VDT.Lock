using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Api.Tests;

public class ResultTests {
    [Fact]
    public async Task ExecuteAsyncWithoutData() {
        var subject = new Result(StatusCodes.Status200OK);

        var httpContext = Substitute.For<HttpContext>();
        var body = Substitute.For<Stream>();
        httpContext.Response.Body.Returns(body);

        await subject.ExecuteAsync(httpContext);

        httpContext.Response.Received().StatusCode = StatusCodes.Status200OK;
        await body.DidNotReceive().WriteAsync(Arg.Any<ReadOnlyMemory<byte>>(), default);
    }

    [Fact]
    public async Task ExecuteAsyncWithData() {
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        var subject = new Result(StatusCodes.Status200OK, data);

        var httpContext = Substitute.For<HttpContext>();
        var body = Substitute.For<Stream>();
        httpContext.Response.Body.Returns(body);

        await subject.ExecuteAsync(httpContext);

        httpContext.Response.Received().StatusCode = StatusCodes.Status200OK;
        await body.Received().WriteAsync(data, default);
    }
}
