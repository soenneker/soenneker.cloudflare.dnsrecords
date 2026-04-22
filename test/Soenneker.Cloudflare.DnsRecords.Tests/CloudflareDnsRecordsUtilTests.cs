using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Cloudflare.DnsRecords.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class CloudflareDnsRecordsUtilTests : HostedUnitTest
{
    private readonly ICloudflareDnsRecordsUtil _util;

    public CloudflareDnsRecordsUtilTests(Host host) : base(host)
    {
        _util = Resolve<ICloudflareDnsRecordsUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
