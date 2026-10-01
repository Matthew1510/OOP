using System;
using System.Collections.Generic;

namespace LogiCore.Domain.Strategies;

/// <summary>
/// Стратегия расчёта тарифов для доставки.
/// </summary>
public interface ITariffStrategy
{
    string Name { get; }

    /// <summary>
    /// Рассчитывает итоговую стоимость по базовой цене, маршруту и составу груза.
    /// </summary>
    decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo);
}

/// <summary>
/// Базовая стратегия расчёта без дополнительных коэффициентов.
/// </summary>
public sealed class StandardTariff : ITariffStrategy
{
    public string Name
    {
        get { return "Обычный тариф"; }
    }

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo)
    {
        ValidateInput(baseCost, route, cargo);
        return baseCost;
    }

    private void ValidateInput(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo)
    {
        if (baseCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(baseCost));
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));
    }
}

/**
 * Стратегия надбавки для тяжёлых грузов.
 */
public sealed class HeavyCargoTariff : ITariffStrategy
{
    private const decimal HeavyCargoLimitKg = 1000m;
    private const decimal HeavyCargoCoefficient = 1.25m;

    public string Name
    {
        get { return "Тариф для тяжёлого груза"; }
    }

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo)
    {
        if (baseCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(baseCost));
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));

        decimal weight = 0m;
        foreach (Cargo item in cargo)
            weight += item.WeightKg;

        if (weight <= HeavyCargoLimitKg)
            return baseCost;

        return baseCost * HeavyCargoCoefficient;
    }
}
