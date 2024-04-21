using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Acmebot.Provider.Loopia.Exceptions;

namespace Acmebot.Provider.Loopia.Loopia
{
    public class LoopiaClient
    {
        public record ZoneRecord(string Type, int Ttl, int Priority, string RData, int RecordId);
        public record Option(string Username, string Password);
        private const string Url = "https://api.loopia.se/RPCSERV";
        private readonly string _username;
        private readonly string _password;
        private readonly HttpClient _httpClient;

        public LoopiaClient(HttpClient httpClient, Option options)
        {
            _username = options.Username;
            _password = options.Password;
            _httpClient = httpClient;
        }

        public async Task<string[]> GetDomains()
        {
            var result = await PostRequest("getDomains");
            using TextReader reader = new StringReader(result);
            return XDocument
                .Load(reader)
                .XPathSelectElements("/methodResponse/params/param/value/array/data/value")
                .Select(x => GetString(x.XPathSelectElements("struct/member"), "domain"))
                .ToArray();
        }

        public async Task<ZoneRecord[]> GetZoneRecords(string domain, string subDomain)
        {
            var result = await PostRequest("getZoneRecords", domain, subDomain);
            using TextReader reader = new StringReader(result);
            return XDocument
                .Load(reader)
                .XPathSelectElements("/methodResponse/params/param/value/array/data/value")
                .Select(x => new ZoneRecord(
                    GetString(x.XPathSelectElements("struct/member"), "type"),
                    GetInt(x.XPathSelectElements("struct/member"), "ttl"),
                    GetInt(x.XPathSelectElements("struct/member"), "priority"),
                    GetString(x.XPathSelectElements("struct/member"), "rdata"),
                    GetInt(x.XPathSelectElements("struct/member"), "record_id")
                ))
                .ToArray();
        }

        public async Task<string?> AddZoneRecord(string domain, string subDomain, string data, int ttl)
        {
            var sb = new StringBuilder();
            sb.Append("<param><value><struct>");
            sb.Append("<member><name>type</name><value><string>TXT</string></value></member>");
            sb.Append($"<member><name>rdata</name><value><string>{data}</string></value></member>");
            sb.Append("<member><name>priority</name><value><i4>0</i4></value></member>");
            sb.Append($"<member><name>ttl</name><value><i4>{ttl}</i4></value></member>");
            sb.Append("</struct></value></param>");
            return GetStringResult(await PostRequest("addZoneRecord", domain, subDomain, sb.ToString()));
        }

        public async Task<string?> RemoveZoneRecord(string domain, string subDomain, int recordId)
        {
            var data = $"<param><value><i4>{recordId}</i4></value></param>";
            return GetStringResult(await PostRequest("removeZoneRecord", domain, subDomain, data));
        }

        public async Task<string?> RemoveSubDomain(string domain, string subDomain) => GetStringResult(await PostRequest("removeSubdomain", domain, subDomain));

        private static string? GetStringResult(string result)
        {
            using TextReader reader = new StringReader(result);
            return XDocument.Load(reader).XPathSelectElement("/methodResponse/params/param/value/string")?.Value;
        }

        private async Task<string> PostRequest(
            string method,
            string? domain = null,
            string? subDomain = null,
            string? data = null)
        {
            var arguments = new List<string>
            {
                _username,
                _password
            };

            if (domain != null)
                arguments.Add(domain);

            if (subDomain != null)
                arguments.Add(subDomain);

            var sb = new StringBuilder();
            sb.Append($"<methodCall><methodName>{method}</methodName><params>");
            foreach (var argument in arguments)
            {
                sb.Append($"<param><value><string>{argument}</string></value></param>");
            }

            if (data != null)
            {
                sb.Append(data);
            }

            sb.Append("</params></methodCall>");
            var result = await _httpClient.PostAsync(Url, new StringContent(sb.ToString(), Encoding.UTF8));
            var content = await result.Content.ReadAsStringAsync();
            if (content.Contains("<methodResponse><fault>"))
                throw new CustomLoopiaException(content);

            return content;
        }

        private static int GetInt(IEnumerable<XElement> member, string name) => int.Parse(GetValue(member, name, "int")!);
        private static string GetString(IEnumerable<XElement> member, string name) => GetValue(member, name, "string")!;
        private static string? GetValue(IEnumerable<XElement> member, string name, string type)
        {
            return member
                .Single(z => z.Element("name")!.Value == name)
                .XPathSelectElement($"value/{type}")?.Value;
        }
    }


}

