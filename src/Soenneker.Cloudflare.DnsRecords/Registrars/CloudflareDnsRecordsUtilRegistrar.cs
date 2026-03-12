using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Cloudflare.Utils.Client.Registrars;

namespace Soenneker.Cloudflare.DnsRecords.Registrars;

/// <summary>
/// A utility for managing Cloudflare DNS records
/// </summary>
public static class CloudflareDnsRecordsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ICloudflareDnsRecordsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflareDnsRecordsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().TryAddSingleton<ICloudflareDnsRecordsUtil, CloudflareDnsRecordsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ICloudflareDnsRecordsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflareDnsRecordsUtilAsScoped(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().TryAddScoped<ICloudflareDnsRecordsUtil, CloudflareDnsRecordsUtil>();

        return services;
    }
}
