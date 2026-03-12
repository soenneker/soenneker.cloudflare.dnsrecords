using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Dns_records;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Dns_records.Item;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    public async ValueTask<Dns_records_dns_response_single> AddARecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(zoneId);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(content);

        _logger.LogInformation("Adding A record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new Dns_records_dns_record_post
        {
            Type = "A",
            AdditionalData = new Dictionary<string, object>
            {
                {"name", name},
                {"content", content},
                {"ttl", ttl},
                {"proxied", proxied}
            }
        };

        return await AddRecord(zoneId, record, cancellationToken).NoSync();
    }

    public async ValueTask<Dns_records_dns_response_single> AddCnameRecord(string zoneId, string name, string content, int ttl = 1, bool proxied = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(zoneId);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(content);

        _logger.LogInformation("Adding CNAME record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new Dns_records_dns_record_post
        {
            Type = "CNAME",
            AdditionalData = new Dictionary<string, object>
            {
                {"name", name},
                {"content", content},
                {"ttl", ttl},
                {"proxied", proxied}
            }
        };

        return await AddRecord(zoneId, record, cancellationToken).NoSync();
    }

    public async ValueTask<Dns_records_dns_response_single> AddTxtRecord(string zoneId, string name, string content, int ttl = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(zoneId);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(content);

        _logger.LogInformation("Adding TXT record for zone {ZoneId}: {Name} -> {Content}", zoneId, name, content);

        var record = new Dns_records_dns_record_post
        {
            Type = "TXT",
            AdditionalData = new Dictionary<string, object>
            {
                {"name", name},
                {"content", content},
                {"ttl", ttl},
                {"proxied", false}
            }
        };

        return await AddRecord(zoneId, record, cancellationToken).NoSync();
    }

    public async ValueTask<Dns_records_dns_response_single> AddMxRecord(string zoneId, string name, string content, int priority, int ttl = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(zoneId);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(content);

        _logger.LogInformation("Adding MX record for zone {ZoneId}: {Name} -> {Content} (Priority: {Priority})", zoneId, name, content, priority);

        var record = new Dns_records_dns_record_post
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
        };

        return await AddRecord(zoneId, record, cancellationToken).NoSync();
    }

    private async ValueTask<Dns_records_dns_response_single> AddRecord(string zoneId, Dns_records_dns_record_post record, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(zoneId);
        ArgumentNullException.ThrowIfNull(record);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken).NoSync();

        try
        {
            Dns_recordsRequestBuilder? dnsRecords = client.Zones[zoneId].Dns_records;
            Dns_records_dns_response_single? result = await dnsRecords.PostAsync(record, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully added {Type} record for zone {ZoneId}: {Name}", record.Type, zoneId, record.AdditionalData["name"]);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add {Type} record for zone {ZoneId}: {Name}", record.Type, zoneId, record.AdditionalData["name"]);
            throw;
        }
    }

    public async ValueTask DeleteRecordById(string zoneId, string recordId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zoneId);
        ArgumentNullException.ThrowIfNull(recordId);

        _logger.LogInformation("Deleting DNS record {RecordId} from zone {ZoneId}", recordId, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken).NoSync();

        try
        {
            WithDns_record_ItemRequestBuilder dnsRecords = client.Zones[zoneId].Dns_records[recordId];
            var body = new WithDns_record_DeleteRequestBody();
            await dnsRecords.DeleteAsync(body, null, cancellationToken).NoSync();
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
        ArgumentNullException.ThrowIfNull(zoneId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);

        _logger.LogInformation("Deleting {Type} record {Name} from zone {ZoneId}", type, name, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            Dns_recordsRequestBuilder dnsRecords = client.Zones[zoneId].Dns_records;
            Dns_records_dns_response_collection? records = await dnsRecords.GetAsync(cancellationToken: cancellationToken).NoSync();

            if (records?.Result == null)
            {
                _logger.LogWarning("No DNS records found in zone {ZoneId}", zoneId);
                return;
            }

            Dns_records_dns_record_response? recordToDelete = records.Result.FirstOrDefault(r =>
                r.AdditionalData.TryGetValue("name", out object? recordName) &&
                recordName?.ToString()?.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                r.AdditionalData.TryGetValue("type", out object? recordType) &&
                recordType?.ToString()?.Equals(type, StringComparison.OrdinalIgnoreCase) == true);

            if (recordToDelete == null)
            {
                _logger.LogWarning("No {Type} record found with name {Name} in zone {ZoneId}", type, name, zoneId);
                return;
            }

            if (recordToDelete.Id == null)
            {
                throw new InvalidOperationException($"Record ID is null for {type} record {name} in zone {zoneId}");
            }

            await DeleteRecordById(zoneId, recordToDelete.Id, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete {Type} record {Name} from zone {ZoneId}", type, name, zoneId);
            throw;
        }
    }

    public async ValueTask DeleteRecordsByType(string zoneId, string type, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zoneId);
        ArgumentNullException.ThrowIfNull(type);

        _logger.LogInformation("Deleting all {Type} records from zone {ZoneId}", type, zoneId);

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken).NoSync();

        try
        {
            Dns_recordsRequestBuilder dnsRecords = client.Zones[zoneId].Dns_records;
            Dns_records_dns_response_collection? records = await dnsRecords.GetAsync(cancellationToken: cancellationToken).NoSync();

            if (records?.Result == null)
            {
                _logger.LogWarning("No DNS records found in zone {ZoneId}", zoneId);
                return;
            }

            IEnumerable<Dns_records_dns_record_response> recordsToDelete = records.Result.Where(r =>
                r.AdditionalData.TryGetValue("type", out object? recordType) &&
                recordType?.ToString()?.Equals(type, StringComparison.OrdinalIgnoreCase) == true);

            foreach (Dns_records_dns_record_response record in recordsToDelete)
            {
                if (record.Id != null)
                {
                    await DeleteRecordById(zoneId, record.Id, cancellationToken).NoSync();
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

    public ValueTask RemoveARecord(string zoneId, string name, CancellationToken cancellationToken = default)
    {
        return DeleteRecordByNameAndType(zoneId, name, "A", cancellationToken);
    }

    public ValueTask RemoveCnameRecord(string zoneId, string name, CancellationToken cancellationToken = default)
    {
        return DeleteRecordByNameAndType(zoneId, name, "CNAME", cancellationToken);
    }

    public ValueTask RemoveTxtRecord(string zoneId, string name, CancellationToken cancellationToken = default)
    {
        return DeleteRecordByNameAndType(zoneId, name, "TXT", cancellationToken);
    }

    public ValueTask RemoveMxRecord(string zoneId, string name, CancellationToken cancellationToken = default)
    {
        return DeleteRecordByNameAndType(zoneId, name, "MX", cancellationToken);
    }
}