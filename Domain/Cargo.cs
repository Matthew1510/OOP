using System;
using LogiCore.Domain.Common;

namespace LogiCore.Domain;

/// <summary>
/// Базовый класс груза, который может быть перевезён логистической системой.
/// </summary>
public abstract class Cargo : IEquatable<Cargo>, IInsurable, IIdentifiable
{
    protected Cargo( string description, decimal weightKg, decimal volumeM3, 
                    decimal declaredValue, DateTime? expirationDate = null){
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("груз не может быть пустым.", nameof(description));
        if (weightKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(weightKg), "груз не может быть нулевым или отрицательным.");
        if (volumeM3 <= 0)
            throw new ArgumentOutOfRangeException(nameof(volumeM3), "объем не может быть нулевым или отрицательным.");
        if (declaredValue < 0)
            throw new ArgumentOutOfRangeException(nameof(declaredValue), "Стоимость не может быть отрицательной.");

        Id = Guid.NewGuid();
        Description = description; 
        WeightKg = weightKg;
        VolumeM3 = volumeM3;
        DeclaredValue = declaredValue;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// Уникальный идентификатор груза.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Описание груза.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Вес груза в килограммах.
    /// </summary>
    public decimal WeightKg { get; }

    /// <summary>
    /// Объём груза в кубических метрах.
    /// </summary>
    public decimal VolumeM3 { get; }

    /// <summary>
    /// Объявленная ценность груза.
    /// </summary>
    public decimal DeclaredValue { get; }

    /// <summary>
    /// Дата истечения срока годности, если груз скоропортящийся.
    /// </summary>
    public DateTime? ExpirationDate { get; }

    decimal IInsurable.GetInsuranceValue()
    {
        return DeclaredValue;
    }

    public bool IsExpired(DateTime? currentDate = null){// проверка срока годности
        return ExpirationDate.HasValue && ExpirationDate.Value.Date < (currentDate ?? DateTime.Today).Date;
    }

//переопределенные методы
    public bool Equals(Cargo? other){
        return other is not null && Id == other.Id;
    }
    public override bool Equals(object? obj){
        return obj is Cargo other && Equals(other);
    }
    public override int GetHashCode(){
        return Id.GetHashCode();
    }
    public override string ToString(){
        return $"{GetType().Name}: {Description} ({WeightKg:0.##} kg, {VolumeM3:0.##} m³)";
    }
}

/**
 * Обычный груз без специальных ограничений.
 */
public sealed class StandardCargo : Cargo
{
    /// <summary>
    /// Создаёт стандартный груз.
    /// </summary>
    public StandardCargo(string description, decimal weightKg, decimal volumeM3, decimal declaredValue)
                : base(description, weightKg, volumeM3, declaredValue, null){}

    /// <summary>
    /// Создаёт стандартный груз с возможным сроком годности.
    /// </summary>

    public StandardCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        DateTime? expirationDate)
                : base(description, weightKg, volumeM3, declaredValue, expirationDate){}
}

/**
 * Груз, требующий соблюдения температурного режима.
 */
public sealed class PerishableCargo : Cargo, ITemperatureSensitive
{
    /// <summary>
    /// Создаёт скоропортящийся груз.
    /// </summary>
    public PerishableCargo(string description, decimal weightKg, decimal volumeM3, decimal declaredValue,DateTime expirationDate, decimal neededTemperature)
                            : base(description, weightKg, volumeM3, declaredValue, expirationDate){
            NeededTemperature = neededTemperature;
        }

    /// <summary>
    /// Требуемая температура хранения груза в градусах Цельсия.
    /// </summary>
    public decimal NeededTemperature {get;}

    /// <summary>
    /// Явно реализует контракт температурно-чувствительного груза.
    /// </summary>
    decimal ITemperatureSensitive.GetNeededTemperature(){
        return NeededTemperature;
    }
}

/**
 * Груз, требующий повышенной аккуратности при транспортировке.
 */
public sealed class FragileCargo : Cargo
{
    /// <summary>
    /// Создаёт хрупкий груз.
    /// </summary>
    public FragileCargo(string description, decimal weightKg, decimal volumeM3, decimal declaredValue, int fragilityLevel = 1)
                            : base(description, weightKg, volumeM3, declaredValue){
        if (fragilityLevel is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(fragilityLevel), "хрупкость должна быть от 1 до 5.");
        FragilityLevel = fragilityLevel;
    }

    /// <summary>
    /// Уровень хрупкости груза от 1 до 5.
    /// </summary>
    public int FragilityLevel { get; }
}

/**
 * Опасный груз с классом опасности от 1 до 9.
 */
public sealed class DangerousCargo : Cargo
{
    /// <summary>
    /// Создаёт опасный груз.
    /// </summary>
    public DangerousCargo(string description, decimal weightKg, decimal volumeM3, decimal declaredValue, int hazardClass)
                     : base(description, weightKg, volumeM3, declaredValue){
        if (hazardClass is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(hazardClass),"Класс опасности должен быть от 1 до 9.");
        HazardClass = hazardClass;
    }

    /// <summary>
    /// Класс опасности груза.
    /// </summary>
    public int HazardClass { get; }
}

/**
 * Сверхгабаритный груз, имеющий три размерных измерения.
 */
public sealed class OversizedCargo : Cargo
{
    /// <summary>
    /// Создаёт сверхгабаритный груз.
    /// </summary>
    public OversizedCargo(string description, decimal weightKg, decimal volumeM3,
                            decimal declaredValue, decimal lengthM, decimal widthM,decimal heightM)
                         : base(description, weightKg, volumeM3, declaredValue){
        if (lengthM <= 0)
            throw new ArgumentOutOfRangeException(nameof(lengthM), "длина должна быть положительной.");
        if (widthM <= 0)
            throw new ArgumentOutOfRangeException(nameof(widthM), "ширина должна быть положительной.");
        if (heightM <= 0)
            throw new ArgumentOutOfRangeException( nameof(heightM),"высота должна быть положительной.");

        LengthM = lengthM;
        WidthM = widthM;
        HeightM = heightM;
    }

    /// <summary>
    /// Длина груза в метрах.
    /// </summary>
    public decimal LengthM { get; }

    /// <summary>
    /// Ширина груза в метрах.
    /// </summary>
    public decimal WidthM { get; }

    /// <summary>
    /// Высота груза в метрах.
    /// </summary>
    public decimal HeightM { get; }
}

/// <summary>
/// Контракт для грузов, для которых важен режим хранения по температуре.
/// </summary>
public interface ITemperatureSensitive
{
    /// <summary>
    /// Возвращает требуемую температуру хранения груза.
    /// </summary>
    decimal GetNeededTemperature();
}

/// <summary>
/// Контракт, определяющий страховую стоимость груза.
/// </summary>
public interface IInsurable
{
    /// <summary>
    /// Возвращает страховую стоимость груза.
    /// </summary>
    decimal GetInsuranceValue();
}
