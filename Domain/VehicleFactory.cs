using System;
using LogiCore.Domain;

namespace LogiCore.Domain.Factories;

/// <summary>
/// Базовая фабрика транспортных средств.
/// </summary>
public abstract class VehicleFactory
{
    /// <summary>
    /// Создаёт транспорт по типу и регистрационному номеру.
    /// </summary>
    public static Vehicle CreateVehicle(string type, string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Тип транспорта не может быть пустым.", nameof(type));


        if (type.Equals("Truck", StringComparison.OrdinalIgnoreCase))
            return new TruckFactory().Create(registrationNumber);
        if (type.Equals("RefrigeratorTruck", StringComparison.OrdinalIgnoreCase))
            return new RefrigeratorTruckFactory().Create(registrationNumber);
        if (type.Equals("CargoPlane", StringComparison.OrdinalIgnoreCase))
            return new CargoPlaneFactory().Create(registrationNumber);
        if (type.Equals("CargoShip", StringComparison.OrdinalIgnoreCase))
            return new CargoShipFactory().Create(registrationNumber);
        if (type.Equals("DroneCourier", StringComparison.OrdinalIgnoreCase))
            return new DroneCourierFactory().Create(registrationNumber);

        throw new ArgumentException($"Неизвестный тип транспорта: {type}.", nameof(type));
    }

    /// <summary>
    /// Создаёт транспорт через фабричный метод.
    /// </summary>
    public Vehicle Create(string registrationNumber){
        return CreateVehicle(registrationNumber);
    }

    /// <summary>
    /// Фабричный метод создания конкретного транспортного средства.
    /// </summary>
    protected abstract Vehicle CreateVehicle(string registrationNumber);
}

/// <summary>
/// Фабрика для создания грузов по типу и параметрам.
/// </summary>
public static class CargoFactory
{
    /// <summary>
    /// Создаёт груз по типу и параметрам.
    /// </summary>
    public static Cargo CreateCargo(
        string type,
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        DateTime? expirationDate = null,
        decimal neededTemperature = 0m,
        int hazardClass = 1,
        int fragilityLevel = 1,
        decimal lengthM = 1m,
        decimal widthM = 1m,
        decimal heightM = 1m)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Тип груза не может быть пустым.", nameof(type));

        if (type.Equals("StandardCargo", StringComparison.OrdinalIgnoreCase))
            return new StandardCargo(description, weightKg, volumeM3, declaredValue, expirationDate);
        if (type.Equals("PerishableCargo", StringComparison.OrdinalIgnoreCase)){
            if (!expirationDate.HasValue)
                throw new ArgumentException("Для скоропортящегося груза нужен срок годности.", nameof(expirationDate));
            return new PerishableCargo(
                description,
                weightKg,
                volumeM3,
                declaredValue,
                expirationDate.Value,
                neededTemperature);
        }
        if (type.Equals("FragileCargo", StringComparison.OrdinalIgnoreCase))
            return new FragileCargo(description, weightKg, volumeM3, declaredValue, fragilityLevel);
        if (type.Equals("DangerousCargo", StringComparison.OrdinalIgnoreCase))
            return new DangerousCargo(description, weightKg, volumeM3, declaredValue, hazardClass);
        if (type.Equals("OversizedCargo", StringComparison.OrdinalIgnoreCase))
            return new OversizedCargo(description, weightKg, volumeM3, declaredValue, lengthM, widthM, heightM);

        throw new ArgumentException($"Неизвестный тип груза: {type}.", nameof(type));
    }
}

/**
 * Фабрика грузового автомобиля.
 */
public sealed class TruckFactory : VehicleFactory
{
    protected override Vehicle CreateVehicle(string registrationNumber){
        return new Truck(registrationNumber);
    }
}

/**
 * Фабрика рефрижераторного грузовика.
 */
public sealed class RefrigeratorTruckFactory : VehicleFactory
{
    protected override Vehicle CreateVehicle(string registrationNumber)
    {
        return new RefrigeratorTruck(registrationNumber);
    }
}

/**
 * Фабрика грузового самолёта.
 */
public sealed class CargoPlaneFactory : VehicleFactory
{
    protected override Vehicle CreateVehicle(string registrationNumber)
    {
        return new CargoPlane(registrationNumber);
    }
}

/**
 * Фабрика грузового судна.
 */
public sealed class CargoShipFactory : VehicleFactory
{
    protected override Vehicle CreateVehicle(string registrationNumber){
        return new CargoShip(registrationNumber);
    }
}

/**
 * Фабрика дрона-курьера.
 */
public sealed class DroneCourierFactory : VehicleFactory
{
    protected override Vehicle CreateVehicle(string registrationNumber){
        return new DroneCourier(registrationNumber);
    }
}
