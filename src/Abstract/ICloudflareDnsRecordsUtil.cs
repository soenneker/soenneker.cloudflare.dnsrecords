using Soenneker.Cloudflare.OpenApiClient.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Cloudflare.DnsRecords.Abstract;

/// <summary>
/// Utility for managing Cloudflare DNS records (A, CNAME, TXT, MX).
/// </summary>
public interface ICloudflareDnsRecordsUtil
{
    /// <summary>
    /// Adds an A record to the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="name">The name of the DNS record (e.g., "example.com").</param>
    /// <param name="content">The IPv4 address.</param>
    /// <param name="ttl">Time to live in seconds (1 = auto).</param>
    /// <param name="proxied">Whether the record should be proxied through Cloudflare.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<DnsRecords_dns_response_single> AddARecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a CNAME record to the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="name">The name of the DNS record (e.g., "www.example.com").</param>
    /// <param name="content">The target domain name.</param>
    /// <param name="ttl">Time to live in seconds (1 = auto).</param>
    /// <param name="proxied">Whether the record should be proxied through Cloudflare.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<DnsRecords_dns_response_single> AddCnameRecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a TXT record to the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="name">The name of the DNS record.</param>
    /// <param name="content">The text content of the record.</param>
    /// <param name="ttl">Time to live in seconds (1 = auto).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<DnsRecords_dns_response_single> AddTxtRecord(string zoneId, string name, string content, int ttl = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an MX record to the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="name">The name of the DNS record.</param>
    /// <param name="content">The mail server hostname.</param>
    /// <param name="priority">The priority of the MX record (lower number = higher priority).</param>
    /// <param name="ttl">Time to live in seconds (1 = auto).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<DnsRecords_dns_response_single> AddMxRecord(string zoneId, string name, string content, int priority, int ttl = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a DNS record by its ID.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="recordId">The ID of the DNS record to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask DeleteRecordById(string zoneId, string recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a DNS record by its name and type.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="name">The name of the DNS record.</param>
    /// <param name="type">The type of the DNS record (e.g., "A", "CNAME", "TXT", "MX").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask DeleteRecordByNameAndType(string zoneId, string name, string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all DNS records of a specific type in the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Cloudflare zone.</param>
    /// <param name="type">The type of DNS records to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask DeleteRecordsByType(string zoneId, string type, CancellationToken cancellationToken = default);
}