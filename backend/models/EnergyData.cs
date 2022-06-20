using System;
using System.Collections.Generic;
using Monocube.RoEnergy.Models;

namespace Monocube.RoEnergy;

public sealed record EnergyData
{
    public DateTime Date { get; init; }
    public IEnumerable<EnergySource> Sources { get; init; }
}