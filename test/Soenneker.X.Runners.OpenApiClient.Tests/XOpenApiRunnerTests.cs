using Soenneker.Tests.HostedUnit;

namespace Soenneker.X.Runners.OpenApiClient.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class XOpenApiRunnerTests : HostedUnitTest
{
    public XOpenApiRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
