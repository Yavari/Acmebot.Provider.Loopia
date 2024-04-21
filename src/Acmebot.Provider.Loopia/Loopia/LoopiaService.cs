using Acmebot.Provider.Loopia.Options;
using Microsoft.Extensions.Logging;
using static Acmebot.Provider.Loopia.FunctionApi;
using static Acmebot.Provider.Loopia.Loopia.LoopiaService;

namespace Acmebot.Provider.Loopia.Loopia
{
    public class LoopiaService
    {
        public record Zone(string Id, string Name);
        private readonly LoopiaClient[] _clients;

        public LoopiaService(HttpClient httpClient, Settings settings)
        {
            _clients = settings.Options
                .Select(x => new LoopiaClient(
                    httpClient,
                    new LoopiaClient.Option(x.Username, x.Password)))
                .ToArray();
        }

        public async Task<IEnumerable<Zone>> GetDomains()
        {
            var zones = new List<Zone>();
            foreach (var client in _clients)
            {
                var domains = await client.GetDomains();
                zones.AddRange(domains.Select(x => new Zone(x, x)));
            }
            return zones;
        }

        public async Task RemoveSubDomain(string zoneId, string recordName)
        {
            foreach (var client in _clients)
            {
                var domains = await client.GetDomains();
                if (domains.Any(x => x == zoneId))
                {
                    await client.RemoveSubDomain(zoneId, recordName.Replace("." + zoneId, ""));
                    return;
                }
            }

            throw new Exception($"{zoneId} not found");
        }

        public async Task AddZoneRecord(string zoneId, string recordName, string[] values)
        {
            var subDomain = recordName.Replace("." + zoneId, "");
            foreach (var client in _clients)
            {
                var domains = await client.GetDomains();
                if (domains.Any(x => x == zoneId))
                {
                    foreach (var value in values)
                    {
                        await client.AddZoneRecord(zoneId, subDomain, value, 300);
                    }
                    return;
                }
            }

            throw new Exception($"{zoneId} not found");
        }
    }
}
