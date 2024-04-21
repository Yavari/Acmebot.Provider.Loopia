using Acmebot.Provider.Loopia.Loopia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Acmebot.Provider.Loopia
{
    public class FunctionApi(ILogger<FunctionApi> logger, LoopiaService loopiaService)
    {
        public record Payload(string Type, int Ttl, string[]? Values);

        [Function(nameof(GetDomains))]
        public async Task<IActionResult> GetDomains(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "zones")] HttpRequest req)
        {
            logger.LogInformation("Request to get domains");
            return new OkObjectResult(await loopiaService.GetDomains());
        }

        [Function(nameof(Delete))]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Admin, "delete", Route = "zones/{zoneId}/records/{recordName}")] HttpRequest req,
            string zoneId,
            string recordName)
        {
            logger.LogInformation($"Request to delete {zoneId} {recordName}");
            await loopiaService.RemoveSubDomain(zoneId, recordName);
            return new OkResult();
        }

        [Function(nameof(Create))]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Admin, "put", Route = "zones/{zoneId}/records/{recordName}")] HttpRequest req,
            string zoneId,
            string recordName)
        {
            var settings = new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true };
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<Payload?>(json, settings);
            if (payload?.Values == null)
            {
                logger.LogError($"Request to create {zoneId} {recordName} but could not get payload {json}");
                return new BadRequestResult();
            }

            logger.LogInformation($"Request to create {zoneId} {recordName} with values {string.Join(",", payload.Values)}");
            await loopiaService.AddZoneRecord(zoneId, recordName, payload.Values);
            return new OkResult();
        }
    }
}
