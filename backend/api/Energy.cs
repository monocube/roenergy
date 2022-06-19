using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Data.Tables;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Monocube.RoEnergy.Models;

namespace Monocube.RoEnergy.Api;

public static class Energy
{
    [FunctionName("GetEnergySources")]
    public static async Task<IActionResult> GetEnergySources(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "energy")] HttpRequest req,
        [Table("energies", Connection = "RoEnergyStorage")] TableClient tableClient)
    {
        var energies = await GetEnergySources(tableClient);
        return new OkObjectResult(energies);
    }
    [FunctionName("UpdateEnergySources")]
    public static async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "update")] HttpRequest req,
        [Table("energies", Connection = "RoEnergyStorage")] ICollector<EnergySourceEntity> rows)
    {
        var energySources = await UpdateDatabase(rows);
        return new OkObjectResult(energySources);
    }
    [FunctionName("TimerUpdate")]
    public static async Task UpdateEnergySources(
        [TimerTrigger("0 0 * * *")] TimerInfo myTimer,
        ILogger log,
        [Table("energies", Connection = "RoEnergyStorage")] ICollector<EnergySourceEntity> rows)
    {
        if (myTimer.IsPastDue)
        {
            log.LogWarning("Timer is past due");
        }
        await UpdateDatabase(rows);
    }
    private static async Task<Dictionary<string, IEnumerable<EnergySource>>> GetEnergySources(TableClient tableClient)
    {
        Dictionary<string, IEnumerable<EnergySource>> energies = new();
        List<EnergySourceEntity> allEnergies = new();
        var energyEntities = tableClient.QueryAsync<EnergySourceEntity>();
        await foreach (var page in energyEntities.AsPages())
        {
            allEnergies.AddRange(page.Values);
        }
        var map = allEnergies.GroupBy(x => x.PartitionKey).ToDictionary(x => x.Key, x => x.Select(x => new EnergySource
        {
            Name = x.Name,
            Capacity = x.Capacity
        }));
        return map;
    }
    private async static Task<IEnumerable<EnergySource>> UpdateDatabase(ICollector<EnergySourceEntity> rows)
    {
        var html = await GetHtml();
        var energySources = GetEnergyTypes(html);
        foreach (var energySource in energySources)
        {
            rows.Add(new EnergySourceEntity()
            {
                PartitionKey = MakeDay(),
                RowKey = energySource.Name,
                Name = energySource.Name,
                Capacity = energySource.Capacity
            });
        };
        return energySources;
    }
    private static string MakeDay()
    {
        return DateTime.UtcNow.ToString("yyyyMMdd");
    }
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
                var capacity = double.Parse(tableRow.ChildNodes[1].InnerText);
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