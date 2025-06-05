using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Dns_records;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Dns_records.Item;

namespace Soenneker.Cloudflare.DnsRecords;

///<inheritdoc cref="ICloudflareDnsRecordsUtil"/>
public sealed class CloudflareDnsRecordsUtil : ICloudflareDnsRecordsUtil
{
    private readonly ICloudflareClientUtil _clientUtil;
    private readonly ILogger<CloudflareDnsRecordsUtil> _logger;

    public CloudflareDnsRecordsUtil(ICloudflareClientUtil client, ILogger<CloudflareDnsRecordsUtil> logger)
    {
        _clientUtil = client;
        _logger = logger;
    }

    public async ValueTask<DnsRecords_dns_response_single> AddARecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding A record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new DnsRecords_dnsRecordPost
        {
            DnsRecordsDnsRecordWithData = new DnsRecords_dnsRecordWithData
            {
                Type = "A",
                AdditionalData = new Dictionary<string, object>
                {
                    {"name", name},
                    {"content", content},
                    {"ttl", ttl},
                    {"proxied", proxied}
                }
            }
        };

        return await AddRecord(zoneId, record, cancellationToken);
    }

    public async ValueTask<DnsRecords_dns_response_single> AddCnameRecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding CNAME record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new DnsRecords_dnsRecordPost
        {
            DnsRecordsDnsRecordWithData = new DnsRecords_dnsRecordWithData
            {
                Type = "CNAME",
                AdditionalData = new Dictionary<string, object>
                {
                    {"name", name},
                    {"content", content},
                    {"ttl", ttl},
                    {"proxied", proxied}
                }
            }
        };

        return await AddRecord(zoneId, record, cancellationToken);
    }

    public async ValueTask<DnsRecords_dns_response_single> AddTxtRecord(string zoneId, string name, string content, int ttl = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding TXT record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new DnsRecords_dnsRecordPost
        {
            DnsRecordsDnsRecordWithData = new DnsRecords_dnsRecordWithData
            {
                Type = "TXT",
                AdditionalData = new Dictionary<string, object>
                {
                    {"name", name},
                    {"content", content},
                    {"ttl", ttl},
                    {"proxied", false}
                }
            }
        };

        return await AddRecord(zoneId, record, cancellationToken);
    }

    public async ValueTask<DnsRecords_dns_response_single> AddMxRecord(string zoneId, string name, string content, int priority, int ttl = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding MX record for zone {ZoneId}: {Name} -> {Content} (Priority: {Priority})", zoneId, name, content, priority);

        var record = new DnsRecords_dnsRecordPost
        {
            DnsRecordsDnsRecordWithData = new DnsRecords_dnsRecordWithData
            {
                Type = "MX",
                AdditionalData = new Dictionary<string, object>
                {
                    {"name", name},
                    {"content", content},
                    {"priority", priority},
                    {"ttl", ttl},
                    {"proxied", false}
                }
            }
        };

        return await AddRecord(zoneId, record, cancellationToken);
    }

    private async ValueTask<DnsRecords_dns_response_single> AddRecord(string zoneId, DnsRecords_dnsRecordPost record, CancellationToken cancellationToken)
    {
        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            Dns_recordsRequestBuilder? dnsRecords = client.Zones[zoneId].Dns_records;
            DnsRecords_dns_response_single? result = await dnsRecords.PostAsync(record, null, cancellationToken);
            _logger.LogInformation("Successfully added {Type} record for zone {ZoneId}: {Name}", 
                record.DnsRecordsDnsRecordWithData?.Type, 
                zoneId, 
                record.DnsRecordsDnsRecordWithData?.AdditionalData["name"]);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add {Type} record for zone {ZoneId}: {Name}", 
                record.DnsRecordsDnsRecordWithData?.Type, 
                zoneId, 
                record.DnsRecordsDnsRecordWithData?.AdditionalData["name"]);
            throw;
        }
    }

    public async ValueTask DeleteRecordById(string zoneId, string recordId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting DNS record {RecordId} from zone {ZoneId}", recordId, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            WithDns_record_ItemRequestBuilder? dnsRecords = client.Zones[zoneId].Dns_records[recordId];
            var body = new Dns_records_for_a_zone_delete_dns_record_RequestBody_application_json();
            await dnsRecords.DeleteAsync(body, null, cancellationToken);
            _logger.LogInformation("Successfully deleted DNS record {RecordId} from zone {ZoneId}", recordId, zoneId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete DNS record {RecordId} from zone {ZoneId}", recordId, zoneId);
            throw;
        }
    }

    public async ValueTask DeleteRecordByNameAndType(string zoneId, string name, string type, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting {Type} record {Name} from zone {ZoneId}", type, name, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            Dns_recordsRequestBuilder? dnsRecords = client.Zones[zoneId].Dns_records;
            DnsRecords_dns_response_collection? records = await dnsRecords.GetAsync(null, null, cancellationToken);

            if (records?.Result == null)
            {
                _logger.LogWarning("No DNS records found in zone {ZoneId}", zoneId);
                return;
            }

            DnsRecords_dnsRecordResponse? recordToDelete = records.Result.FirstOrDefault(r =>
                r.AdditionalData.TryGetValue("name", out object? recordName) &&
                recordName?.ToString()?.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                r.AdditionalData.TryGetValue("type", out object? recordType) &&
                recordType?.ToString()?.Equals(type, StringComparison.OrdinalIgnoreCase) == true);

            if (recordToDelete == null)
            {
                _logger.LogWarning("No {Type} record found with name {Name} in zone {ZoneId}", type, name, zoneId);
                return;
            }

            await DeleteRecordById(zoneId, recordToDelete.Id?.ToString() ?? throw new InvalidOperationException("Record ID is null"), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete {Type} record {Name} from zone {ZoneId}", type, name, zoneId);
            throw;
        }
    }

    public async ValueTask DeleteRecordsByType(string zoneId, string type, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting all {Type} records from zone {ZoneId}", type, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            Dns_recordsRequestBuilder? dnsRecords = client.Zones[zoneId].Dns_records;
            DnsRecords_dns_response_collection? records = await dnsRecords.GetAsync(null, null, cancellationToken);

            if (records?.Result == null)
            {
                _logger.LogWarning("No DNS records found in zone {ZoneId}", zoneId);
                return;
            }

            IEnumerable<DnsRecords_dnsRecordResponse> recordsToDelete =
                records.Result.Where(r => 
                    r.AdditionalData.TryGetValue("type", out object? recordType) &&
                    recordType?.ToString()?.Equals(type, StringComparison.OrdinalIgnoreCase) == true);

            foreach (DnsRecords_dnsRecordResponse record in recordsToDelete)
            {
                if (record.Id != null)
                {
                    await DeleteRecordById(zoneId, record.Id.ToString(), cancellationToken);
                }
            }

            _logger.LogInformation("Successfully deleted all {Type} records from zone {ZoneId}", type, zoneId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all {Type} records from zone {ZoneId}", type, zoneId);
            throw;
        }
    }
}