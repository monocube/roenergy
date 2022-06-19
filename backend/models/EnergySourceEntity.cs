using System;
using Azure;
using Azure.Data.Tables;

namespace Monocube.RoEnergy.Models;

public sealed class EnergySourceEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Name { get; set; }
    public double Capacity { get; set; }
}