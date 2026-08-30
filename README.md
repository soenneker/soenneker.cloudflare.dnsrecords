[![](https://img.shields.io/nuget/v/soenneker.cloudflare.dnsrecords.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.dnsrecords/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.dnsrecords/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.dnsrecords/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.cloudflare.dnsrecords.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.dnsrecords/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.dnsrecords/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.dnsrecords/actions/workflows/codeql.yml)

# Soenneker.Cloudflare.DnsRecords

Creates and deletes A, CNAME, TXT, and MX records in Cloudflare zones.

## Installation

```bash
dotnet add package Soenneker.Cloudflare.DnsRecords
```

## Configuration

```json
{
  "Cloudflare": {
    "ApiKey": "your-api-token"
  }
}
```

Use a scoped API token with DNS read and edit permission for the zones the application manages.

## Registration

```csharp
using Soenneker.Cloudflare.DnsRecords.Registrars;

services.AddCloudflareDnsRecordsUtilAsScoped();
```

The scoped utility shares the singleton Cloudflare client utility. Singleton registration is also available.

## Creating records

```csharp
using Soenneker.Cloudflare.DnsRecords.Abstract;

public sealed class DnsProvisioner(ICloudflareDnsRecordsUtil dns)
{
    public async ValueTask AddAppRecord(
        string zoneId,
        string address,
        CancellationToken cancellationToken)
    {
        await dns.AddARecord(
            zoneId,
            "app.example.com",
            address,
            ttl: 1,
            proxied: true,
            cancellationToken);
    }
}
```

TTL `1` asks Cloudflare to manage the TTL automatically. TXT and MX records are always created with proxying disabled.

## Deleting records

```csharp
await dns.RemoveCnameRecord(zoneId, "old.example.com", cancellationToken);
await dns.DeleteRecordById(zoneId, recordId, cancellationToken);
```

`DeleteRecordByNameAndType` removes the first matching record returned by Cloudflare and does nothing when none is found. `DeleteRecordsByType` enumerates every result page before deleting all records of that type; this is destructive, so avoid broad record types unless deleting the entire set is intentional.

Cloudflare API failures are logged and rethrown. Callers should handle Kiota/API exceptions around provisioning workflows that need rollback or retry behavior.
