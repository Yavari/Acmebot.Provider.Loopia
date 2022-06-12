using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acmebot.Provider.Loopia.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Acmebot.Provider.Loopia
{
    public static class Function
    {
        public record Zone(string Id, string Name);
        public record Payload(string Type, int Ttl, string[] Values);

        [FunctionName(nameof(GetDomains))]
        public static async Task<ActionResult<Zone[]>> GetDomains(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "zones")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request to get domains");
            var client = CreateLoopiaClient();
            var domains = await client.GetDomains();
            var zones = domains.Select(x => new Zone(x, x));
            return zones.ToArray();
        }

        [FunctionName(nameof(Delete))]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Admin, "delete", Route = "zones/{zoneId}/records/{recordName}")]
            HttpRequest req,
            string zoneId,
            string recordName,
            ILogger log)
        {
            var subDomain = recordName.Replace("." + zoneId, "");
            log.LogInformation($"Request to delete {zoneId} {subDomain}");
            var client = CreateLoopiaClient();
            await client.RemoveSubDomain(zoneId, recordName);
            return new OkResult();
        }

        [FunctionName(nameof(Create))]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Admin, "put", Route = "zones/{zoneId}/records/{recordName}")]
            HttpRequest req,
            string zoneId,
            string recordName,
            ILogger log)
        {
            var client = CreateLoopiaClient();
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonConvert.DeserializeObject<Payload>(content);
            var subDomain = recordName.Replace("." + zoneId, "");
            log.LogInformation($"Request to create {zoneId} {subDomain} with values {string.Join(",", payload.Values)}");
            foreach (var value in payload.Values)
            {
                await client.AddZoneRecord(zoneId, subDomain, value, 300);
            }

            return new OkResult();
        }


        private static LoopiaClient CreateLoopiaClient()
        {
            var username = Environment.GetEnvironmentVariable("Loopia:Username");
            var password = Environment.GetEnvironmentVariable("Loopia:Password");
            if (string.IsNullOrEmpty(username))
                throw new CustomConfigurationException($"{nameof(username)} is required");

            if (string.IsNullOrEmpty(password))
                throw new CustomConfigurationException($"{nameof(password)} is required");

            var client = new LoopiaClient(new HttpClient(), new LoopiaClient.Option(username, password));
            return client;
        }
    }


}
