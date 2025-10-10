using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Api.Tests;

public class RawOkResultTests {
    [Fact]
    public async Task ExecuteAsyncWithData() {
        var value = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        var subject = new RawOkResult(value);

        var httpContext = Substitute.For<HttpContext>();
        var body = Substitute.For<Stream>();
        httpContext.Response.Body.Returns(body);

        await subject.ExecuteAsync(httpContext);

        httpContext.Response.Received().StatusCode = StatusCodes.Status200OK;
        await body.Received().WriteAsync(value, default);
    }
}
