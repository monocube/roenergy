namespace Monocube.RoEnergy.Models;

public sealed record EnergySource
{
    public string Name { get; init; }
    public float Capacity { get; init; }
}