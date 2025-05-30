using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Cloudflare.DnsRecords.Tests;

[Collection("Collection")]
public sealed class CloudflareDnsRecordsUtilTests : FixturedUnitTest
{
    private readonly ICloudflareDnsRecordsUtil _util;

    public CloudflareDnsRecordsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflareDnsRecordsUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
