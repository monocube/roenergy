using System;
using Microsoft.Azure.Cosmos.Table;

namespace Monocube.RoEnergy.Models;

public sealed class EnergySourceEntity : TableEntity
{
    [IgnoreProperty]
    public string Name => RowKey;
    public double Capacity { get; set; }
}