using System;
using System.Collections.Generic;
using System.Linq;

interface IHazardNotifier
{
    void NotifyHazard(string message);
}

abstract class Container
{
    private static int counter = 1;
    public string SerialNumber { get; }
    public double MaxLoad { get; protected set; }
    public double CurrentLoad { get; protected set; }

    protected Container(string type, double maxLoad)
    {
        SerialNumber = $"KON-{type}-{counter++}";
        MaxLoad = maxLoad;
    }

    public virtual void Load(double weight)
    {
        if (CurrentLoad + weight > MaxLoad)
            throw new Exception("OverfillException: Load exceeds maximum capacity");
        CurrentLoad += weight;
    }

    public virtual void Unload()
    {
        CurrentLoad = 0;
    }
}

class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; }

    public LiquidContainer(double maxLoad, bool isHazardous) : base("L", maxLoad)
    {
        IsHazardous = isHazardous;
    }

    public override void Load(double weight)
    {
        double limit = IsHazardous ? MaxLoad * 0.5 : MaxLoad * 0.9;
        if (CurrentLoad + weight > limit)
        {
            NotifyHazard("Hazardous operation attempt!");
            return;
        }
        base.Load(weight);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Hazard Alert [{SerialNumber}]: {message}");
    }
}

class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; }

    public GasContainer(double maxLoad, double pressure) : base("G", maxLoad)
    {
        Pressure = pressure;
    }

    public override void Unload()
    {
        CurrentLoad *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Hazard Alert [{SerialNumber}]: {message}");
    }
}

class RefrigeratedContainer : Container
{
    public string ProductType { get; }
    public double RequiredTemperature { get; }

    public RefrigeratedContainer(double maxLoad, string productType, double requiredTemperature)
        : base("C", maxLoad)
    {
        ProductType = productType;
        RequiredTemperature = requiredTemperature;
    }
}

class Ship
{
    public List<Container> Containers { get; } = new List<Container>();
    public double MaxWeight { get; }
    public int MaxContainers { get; }
    public double MaxSpeed { get; }

    public Ship(double maxWeight, int maxContainers, double maxSpeed)
    {
        MaxWeight = maxWeight;
        MaxContainers = maxContainers;
        MaxSpeed = maxSpeed;
    }

    public void LoadContainer(Container container)
    {
        if (Containers.Count >= MaxContainers || Containers.Sum(c => c.MaxLoad) + container.MaxLoad > MaxWeight)
            throw new Exception("Ship overload or too many containers");
        Containers.Add(container);
    }

    public void UnloadContainer(string serialNumber)
    {
        Containers.RemoveAll(c => c.SerialNumber == serialNumber);
    }

    public void TransferContainer(string serialNumber, Ship targetShip)
    {
        var container = Containers.FirstOrDefault(c => c.SerialNumber == serialNumber);
        if (container == null) return;
        UnloadContainer(serialNumber);
        targetShip.LoadContainer(container);
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Ship Info: Max Speed = {MaxSpeed} knots, Containers Count = {Containers.Count}");
        foreach (var container in Containers)
            Console.WriteLine($"  - {container.SerialNumber}, Load: {container.CurrentLoad}/{container.MaxLoad}");
    }
}

class Program
{
    static void Main()
    {
        Ship ship1 = new Ship(5000, 10, 30);
        Ship ship2 = new Ship(6000, 12, 28);

        Container container1 = new LiquidContainer(2000, true);
        Container container2 = new GasContainer(1500, 5);
        Container container3 = new RefrigeratedContainer(1000, "Bananas", 5);

        ship1.LoadContainer(container1);
        ship1.LoadContainer(container2);
        ship1.LoadContainer(container3);

        container1.Load(800);
        container2.Load(1400);
        container3.Load(900);

        ship1.PrintInfo();
        ship1.TransferContainer(container3.SerialNumber, ship2);

        Console.WriteLine("After Transfer:");
        ship1.PrintInfo();
        ship2.PrintInfo();

        Console.WriteLine("Type 'exit' to close the program.");
        while (Console.ReadLine()?.ToLower() != "exit")
        {
            Console.WriteLine("Invalid input. Type 'exit' to close the program.");
        }
    }
}
