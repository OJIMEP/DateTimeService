using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DateTimeService.Data
{
    public class GetGeoZone    {
        
        public static async Task<int> SendRequestToServiceAsync(long latitude, long longitude, IConfiguration _configuration)
        {
            string connString = _configuration.GetConnectionString("BTS_zones");

            var request = new HttpRequestMessage(HttpMethod.Post,
            connString);
            request.Headers.Add("Content-Type", "text/xml");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            string content = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <m:getZoneByCoords xmlns:m=""http://ws.vrptwserver.beltranssat.by/"">
            <m:latitude xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">{0}</m:latitude>
            <m:longitude xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">{1}</m:longitude>
            <m:geomNeeded xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">false</m:geomNeeded>
        </m:getZoneByCoords>
    </soap:Body>
</soap:Envelope>";

            content = string.Format(content, latitude, longitude);

            request.Content = new StringContent(content, Encoding.UTF8, "text/xml");

            var client = new HttpClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                //Branches = await XmlSerializer.
                //    <IEnumerable<GitHubBranch>>(responseStream);
            }
            else
            {
                //GetBranchesError = true;
                //Branches = Array.Empty<GitHubBranch>();
            }


            return 0;
        }
    }
}
