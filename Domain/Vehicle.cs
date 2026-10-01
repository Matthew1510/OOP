using System;
using System.Collections.Generic;
using LogiCore.Domain.Common;

namespace LogiCore.Domain;

/// <summary>
/// Состояние транспортного средства в системе.
/// </summary>
public enum VehicleState
{
    /// <summary>
    /// Транспорт свободен и может быть назначен на заказ.
    /// </summary>
    Free,

    /// <summary>
    /// Транспорт выполняет доставку.
    /// </summary>
    InTransit,

    /// <summary>
    /// Транспорт находится на техническом обслуживании.
    /// </summary>
    UnderMaintenance
}

/// <summary>
/// Базовый класс транспортного средства.
/// </summary>
public abstract class Vehicle : IEquatable<Vehicle>, IIdentifiable
{
    protected Vehicle(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new ArgumentException("Необходим регистрационный номер.", nameof(registrationNumber));
        if (maxLoadKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLoadKg), "Грузоподъемность должна быть положительной.");
        if (maxVolumeM3 <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxVolumeM3), "Объем должен быть положительным.");
        if (averageSpeedKmH <= 0)
            throw new ArgumentOutOfRangeException(nameof(averageSpeedKmH), "Скорость должна быть положительной.");
        if (baseRatePerKm < 0)
            throw new ArgumentOutOfRangeException(nameof(baseRatePerKm), "Тариф не может быть отрицательным.");

        Id = Guid.NewGuid();
        RegistrationNumber = registrationNumber;
        MaxLoadKg = maxLoadKg;
        MaxVolumeM3 = maxVolumeM3;
        AverageSpeedKmH = averageSpeedKmH;
        BaseRatePerKm = baseRatePerKm;
        State = VehicleState.Free;
    }

    /// <summary>
    /// Уникальный идентификатор автомобиля.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Государственный регистрационный номер.
    /// </summary>
    public string RegistrationNumber { get; }

    /// <summary>
    /// Максимальная грузоподъёмность в килограммах.
    /// </summary>
    public decimal MaxLoadKg { get; }

    /// <summary>
    /// Максимальный объём груза в кубических метрах.
    /// </summary>
    public decimal MaxVolumeM3 { get; }

    /// <summary>
    /// Средняя скорость транспортного средства в км/ч.
    /// </summary>
    public decimal AverageSpeedKmH { get; }

    /// <summary>
    /// Базовая ставка за километр.
    /// </summary>
    public decimal BaseRatePerKm { get; set; }

    /// <summary>
    /// Текущее состояние транспортного средства.
    /// </summary>
    public VehicleState State { get; protected set; }

    /// <summary>
    /// Проверяет, подходит ли груз для транспортного средства.
    /// </summary>
    public virtual bool CanCarry(Cargo cargo){
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));
        return State == VehicleState.Free && cargo.WeightKg <= MaxLoadKg && cargo.VolumeM3 <= MaxVolumeM3;
    }

    /// <summary>
    /// Рассчитывает стоимость перевозки для заданного маршрута и состава груза.
    /// </summary>
    public abstract decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo);

    /// <summary>
    /// Вычисляет базовую стоимость перевозки по маршруту с учётом загруженности.
    /// </summary>
    protected decimal CalculateBaseDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo, decimal loadSurchargeRate = 0.05m,bool useDirectDistance = false){
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));
        decimal totalWeightKg = 0m;
        decimal totalVolumeM3 = 0m;

        foreach (Cargo item in cargo){
            if (!CanCarry(item))
                throw new InvalidOperationException($"Транспорт '{RegistrationNumber}' не может перевозить груз '{item.Description}'.");
            totalWeightKg += item.WeightKg;
            totalVolumeM3 += item.VolumeM3;
        }

        if (totalWeightKg > MaxLoadKg || totalVolumeM3 > MaxVolumeM3)
            throw new InvalidOperationException($"Транспорт '{RegistrationNumber}' перегружен.");
        decimal distanceKm = route.DistanceKm;
        if (useDirectDistance)
            distanceKm = route.DirectDistanceKm;

        decimal baseCost = distanceKm * BaseRatePerKm;// базовая стоимость доставки
        decimal loadRatio = totalWeightKg / MaxLoadKg;// коэффициент загрузки
        decimal loadSurcharge = baseCost * loadRatio * loadSurchargeRate;// надбавка за загрузку

        return baseCost + loadSurcharge;
    }

    /// <summary>
    /// Назначает транспорт на заказ.
    /// </summary>
    public void Assign()
    {
         if (State != VehicleState.Free)
            throw new InvalidOperationException($"Транспорт должен быть в состоянии Free, текущее состояние: '{State}'.");
    }

    /// <summary>
    /// Запускает перевозку.
    /// </summary>
    public void StartDelivery()
    {
        if (State != VehicleState.Free)
            throw new InvalidOperationException($"Транспорт должен быть в состоянии Free, текущее состояние: '{State}'.");
        State = VehicleState.InTransit;
    }

    /// <summary>
    /// Освобождает транспорт после завершения или отмены заказа.
    /// </summary>
    public void Release()
    {
        if (State == VehicleState.UnderMaintenance)
            throw new InvalidOperationException("Транспорт на техобслуживании нельзя освободить.");

        State = VehicleState.Free;
    }

    /// <summary>
    /// Переводит транспорт на техническое обслуживание.
    /// </summary>
    public void SetMaintenance()
    {
        if (State == VehicleState.InTransit)
            throw new InvalidOperationException("Транспорт в пути нельзя отправить на техобслуживание.");
        State = VehicleState.UnderMaintenance;
    }


// переопределенные методы
    public bool Equals(Vehicle? other){
        return other is not null && Id == other.Id;
    }
    public override bool Equals(object? obj) {
        return obj is Vehicle other && Equals(other);
    }
    public override int GetHashCode(){
        return Id.GetHashCode();
    }
    public override string ToString(){
        return $"{GetType().Name} {RegistrationNumber} ({MaxLoadKg:0.##} kg, {MaxVolumeM3:0.##} m3)";
    }
}

/**
 * Грузовой автомобиль с учётом платных участков маршрута.
 */
public class Truck : Vehicle
{
    /// <summary>
    /// Создаёт грузовой автомобиль.
    /// </summary>
    public Truck(string registrationNumber): this(registrationNumber, 20_000m, 100m, 80m, 1.2m){ }

    protected Truck(string registrationNumber,decimal maxLoadKg,decimal maxVolumeM3,decimal averageSpeedKmH,decimal baseRatePerKm): 
        base(registrationNumber,maxLoadKg,maxVolumeM3,averageSpeedKmH,baseRatePerKm){
        TollRoadCoefficient = LogisticsSettings.Instance.DefaultTollRoadCoefficient;
        TollRoadDistanceKm = 0m;
    }

    /// <summary>
    /// Коэффициент стоимости проезда по платным участкам.
    /// </summary>
    public decimal TollRoadCoefficient { get; set; }

    /// <summary>
    /// Длина платного участка маршрута в километрах.
    /// </summary>
    public decimal TollRoadDistanceKm { get; set; }

    public override decimal CalculateDeliveryCost( Route route, IReadOnlyCollection<Cargo> cargo){
        ValidateTollRoadSettings(route);

        decimal baseCost = CalculateBaseDeliveryCost(route, cargo, 0.20m);
        decimal tollCost = TollRoadDistanceKm * BaseRatePerKm * TollRoadCoefficient;

        return baseCost + tollCost;
    }

    private void ValidateTollRoadSettings(Route route){// проверка настроек платной дороги
        if (TollRoadCoefficient < 1m)
            throw new InvalidOperationException("Коэффициент платной дороги должен быть не меньше 1.");
        if (TollRoadDistanceKm < 0m || TollRoadDistanceKm > route.DistanceKm)
            throw new InvalidOperationException( "Длина платного участка должна быть от 0 до длины маршрута.");
    }
}

/**
 * Грузовик с поддержкой определённого температурного диапазона.
 */
public class RefrigeratorTruck : Truck
{
    /// <summary>
    /// Создаёт рефрижераторный грузовик.
    /// </summary>
    public RefrigeratorTruck(string registrationNumber,decimal minTemperature = -25m,decimal maxTemperature = 8m)
                    : base(registrationNumber, 10_000m, 50m, 80m, 2m){
        if (minTemperature > maxTemperature)
            throw new ArgumentException("Минимальная температура не может быть выше максимальной.");
        MinTemperatureC = minTemperature;
        MaxTemperatureC = maxTemperature;
    }

    /// <summary>
    /// Минимально допустимая температура хранения груза.
    /// </summary>
    public decimal MinTemperatureC { get; }

    /// <summary>
    /// Максимально допустимая температура хранения груза.
    /// </summary>
    public decimal MaxTemperatureC { get; }
    public override bool CanCarry(Cargo cargo){
        if (!base.CanCarry(cargo))
            return false;
        if (cargo is ITemperatureSensitive temperatureSensitive){
            decimal requiredTemperature = temperatureSensitive.GetNeededTemperature();// требуемая температура
            return requiredTemperature >= MinTemperatureC && requiredTemperature <= MaxTemperatureC;
        }

        return true;
    }

    public bool CanCarry(decimal weightKg, decimal volumeM3, decimal cargoTemperatureC){// проверка возможности перевозки груза с учетом температуры
        return cargoTemperatureC >= MinTemperatureC && cargoTemperatureC <= MaxTemperatureC && weightKg <= MaxLoadKg
            && volumeM3 <= MaxVolumeM3&& State == VehicleState.Free;
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo){
        return base.CalculateDeliveryCost(route, cargo) * 1.15m;//считаем  стоимость с коэфф 1.15
    }
}

/**
 * Грузовой самолёт с высокой скоростью и ограничениями по типам грузов.
 */
public class CargoPlane : Vehicle
{
    /// <summary>
    /// Создаёт грузовой самолёт.
    /// </summary>
    public CargoPlane(string registrationNumber): base(registrationNumber, 10_000m, 1_000m, 850m, 10m) { }

    public override bool CanCarry(Cargo cargo)
    {
        if (!base.CanCarry(cargo))// проверяем базовые условия
            return false;

        return cargo is not FragileCargo;// груз не должен быть хрупким
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) {
        return CalculateBaseDeliveryCost(route, cargo, 0.10m, true) * 1.80m;
    }
}

/**
 * Морской грузовой транспорт с большим объёмом и низкой скоростью.
 */
public class CargoShip : Vehicle
{
    /// <summary>
    /// Создаёт грузовое судно.
    /// </summary>
    public CargoShip(string registrationNumber)
        : base(registrationNumber, 20_000_000m, 100_000m, 30m, 1m){}

    public override decimal CalculateDeliveryCost( Route route,IReadOnlyCollection<Cargo> cargo){
        return CalculateBaseDeliveryCost(route, cargo, 0.05m);
    }
}

/**
 * Дрон для доставки небольших и лёгких грузов на ограниченную дальность.
 */
public sealed class DroneCourier : Vehicle
{
    /// <summary>
    /// Создаёт дрон-курьера.
    /// </summary>
    public DroneCourier(string registrationNumber) : base(registrationNumber, 30m, 0.15m, 60m, 4m)
    {
        MaxFlightDistanceKm = LogisticsSettings.Instance.DroneMaxFlightDistanceKm;
    }

    /// <summary>
    /// Максимальная дальность полёта дрона в километрах.
    /// </summary>
    public decimal MaxFlightDistanceKm { get; }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo){
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (route.DirectDistanceKm > MaxFlightDistanceKm)
            throw new InvalidOperationException("ясДальность полёта дрона не может превышать 50 км.");
        return CalculateBaseDeliveryCost(route, cargo, 0.05m, true) * 1.35m;
    }
}