using System;

namespace Monocube.RoEnergy.Models;

public sealed record EnergySource
{
    public string Name { get; init; }
    public double Capacity { get; init; }
}