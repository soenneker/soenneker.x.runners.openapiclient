using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.X.Runners.OpenApiClient.Tests;

[Collection("Collection")]
public sealed class XOpenApiRunnerTests : FixturedUnitTest
{
    public XOpenApiRunnerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
