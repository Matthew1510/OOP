using System;
using System.Collections.Generic;
using LogiCore.Domain;
using LogiCore.Domain.Strategies;

namespace LogiCore.Plugins;

/// <summary>
/// Сезонная стратегия тарифов, подключаемая как внешний плагин.
/// </summary>
[TariffStrategy("Плагин: сезонный тариф")]
public sealed class SeasonalTariff : ITariffStrategy
{
    public string Name => "Плагин: сезонный тариф";

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo){
        if (baseCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(baseCost));
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));

        decimal perishableCost = 0m;
        foreach (Cargo item in cargo)
        {
            if (item is PerishableCargo)
                perishableCost += item.DeclaredValue * 0.02m;
        }

        return baseCost + perishableCost;
    }
}
