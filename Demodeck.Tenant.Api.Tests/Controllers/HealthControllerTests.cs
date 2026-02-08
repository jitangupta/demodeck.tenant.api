using Demodeck.Tenant.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Demodeck.Tenant.Api.Tests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _controller = new HealthController();
    }

    [Fact]
    public void Get_ReturnsOkResult()
    {
        var result = _controller.Get();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Get_StatusIsHealthy()
    {
        var result = _controller.Get() as OkObjectResult;
        var value = result!.Value!;
        var status = value.GetType().GetProperty("status")!.GetValue(value);
        Assert.Equal("Healthy", status);
    }

    [Fact]
    public void Get_ServiceNameIsCorrect()
    {
        var result = _controller.Get() as OkObjectResult;
        var value = result!.Value!;
        var service = value.GetType().GetProperty("service")!.GetValue(value);
        Assert.Equal("Demodeck.Tenant.Api", service);
    }

    [Fact]
    public void Get_VersionIs1_0_0()
    {
        var result = _controller.Get() as OkObjectResult;
        var value = result!.Value!;
        var version = value.GetType().GetProperty("version")!.GetValue(value);
        Assert.Equal("1.0.0", version);
    }

    [Fact]
    public void Get_TimestampIsRecentUtc()
    {
        var before = DateTime.UtcNow;
        var result = _controller.Get() as OkObjectResult;
        var after = DateTime.UtcNow;
        var value = result!.Value!;
        var timestamp = (DateTime)value.GetType().GetProperty("timestamp")!.GetValue(value)!;
        Assert.InRange(timestamp, before, after);
    }

    [Fact]
    public void Get_ReturnsExpectedProperties()
    {
        var result = _controller.Get() as OkObjectResult;
        var value = result!.Value!;
        var type = value.GetType();
        Assert.NotNull(type.GetProperty("status"));
        Assert.NotNull(type.GetProperty("service"));
        Assert.NotNull(type.GetProperty("timestamp"));
        Assert.NotNull(type.GetProperty("version"));
    }
}
