using System;

namespace Monocube.RoEnergy.Models;

public sealed class EnergySourceEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    //public ETag ETag { get; set; }
    public string Name { get; set; }
    public double Capacity { get; set; }
}