using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Monocube.RoEnergy.Models;

namespace Monocube.RoEnergy.Api
{
    public static class Energy
    {
        [FunctionName("Energy")]
        public static async Task<IActionResult> GetEnergySources(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }
        //TODO: get back to this later, once the Azure resources are created
        /*        [FunctionName("UpdateEnergySources")]
                public static async Task<IActionResult> UpdateEnergySources([TimerTrigger("0 0 * * *")] TimerInfo myTimer, ILogger log, [Table("energies")] ICollector<EnergySourceEntity> rows)
                {
                    if (myTimer.IsPastDue)
                    {
                        log.LogWarning("Timer is past due");
                    }
                    var html = await GetHtml();
                    var energySources = GetEnergyTypes(html);
                    foreach(var energySource in energySources)
                    {
                        rows.Add(new EnergySourceEntity()
                        {
                            PartitionKey = "",
                            RowKey = energySource.Name,
                            Name = energySource.Name,
                            Capacity = energySource.Capacity
                        });
                    };
                    return new OkResult();
                }
        */
        private static Task<string> GetHtml()
        {
            //TODO: switch to HttpClientFactory
            var client = new HttpClient();
            return client.GetStringAsync("https://www.anre.ro/ro/energie-electrica/rapoarte/puterea-instalata-in-capacitatiile-de-productie-energie-electrica");
        }
        private static IEnumerable<EnergySource> GetEnergyTypes(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var tableRows = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"container\"]/div[2]/div[1]/div/div[2]/table/tr");
            List<EnergySource> data = new();
            foreach (var tableRow in tableRows)
            {
                if (tableRow.ChildNodes.Count == 2) //ugly, make sure it works first
                {
                    var name = tableRow.ChildNodes[0].InnerText;
                    var capacity = float.Parse(tableRow.ChildNodes[1].InnerText);
                    data.Add(new EnergySource()
                    {
                        Name = name,
                        Capacity = capacity
                    });
                }
            }
            return data;
        }
    }
}
