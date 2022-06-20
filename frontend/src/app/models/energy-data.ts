import { IEnergySource } from "./energy-source";

export interface IEnergyData
{
    date: string;
    sources: IEnergySource[];
}