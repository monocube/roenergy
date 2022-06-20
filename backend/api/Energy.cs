using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Monocube.RoEnergy.Models;

namespace Monocube.RoEnergy.Api;

public static class Energy
{
    private const string TableName = "energies";
    private const string ConnectionName = "RoEnergyStorage";
    private const string EnergySourceURL = "https://www.anre.ro/ro/energie-electrica/rapoarte/puterea-instalata-in-capacitatiile-de-productie-energie-electrica";
    private const string EnergyXPath = "//*[@id=\"container\"]/div[2]/div[1]/div/div[2]/table/tr";
    [FunctionName("GetEnergySources")]
    public static async Task<IActionResult> GetEnergySources(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "energy")] HttpRequest req,
        [Table(TableName, Connection = ConnectionName)] CloudTable table)
    {
        var energies = await GetEnergySources(table);
        return new OkObjectResult(energies);
    }
    [FunctionName("UpdateEnergySources")]
    public static async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "update")] HttpRequest req,
        [Table(TableName, Connection = ConnectionName)] ICollector<EnergySourceEntity> rows,
        [Table(TableName, Connection = ConnectionName)] CloudTable table)
    {
        if (await EnergySourcesUpdated(table))
        {
            return new OkObjectResult("Already updated.");
        }
        var energySources = await UpdateDatabase(rows);
        return new CreatedResult("/energy", energySources);
    }
    //Azure Static Web Apps don't yet support Timer Triggers
    /*
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
    */
    private static async Task<Dictionary<string, IEnumerable<EnergySource>>> GetEnergySources(CloudTable table)
    {
        //It seems Azure hosted functions doesn't yet support the new Azure Storage SDK packages
        /*        Dictionary<string, IEnumerable<EnergySource>> energies = new();
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
        */
        TableQuerySegment<EnergySourceEntity> querySegment = null;
        var entities = new List<EnergySourceEntity>();
        var query = new TableQuery<EnergySourceEntity>();
        do
        {
            querySegment = await table.ExecuteQuerySegmentedAsync(query, querySegment?.ContinuationToken);
            entities.AddRange(querySegment.Results);
        } while (querySegment?.ContinuationToken != null);
        var map = entities.GroupBy(x => x.PartitionKey).ToDictionary(x => x.Key, x => x.Select(x => new EnergySource
        {
            Name = x.Name,
            Capacity = x.Capacity
        }));
        return map;
    }
    private static async Task<bool> EnergySourcesUpdated(CloudTable table)
    {
        var today = MakeDay();
        TableQuerySegment<EnergySourceEntity> querySegment = null;
        var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, today);
        var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, "Solar");
        var query = new TableQuery<EnergySourceEntity>().Where(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter));
        do
        {
            var entities = await table.ExecuteQuerySegmentedAsync(query, querySegment?.ContinuationToken);
            if (entities.Any())
            {
                return true;
            }
        } while (querySegment?.ContinuationToken != null);
        return false;
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
        return client.GetStringAsync(EnergySourceURL);
    }
    private static IEnumerable<EnergySource> GetEnergyTypes(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var tableRows = htmlDoc.DocumentNode.SelectNodes(EnergyXPath);
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