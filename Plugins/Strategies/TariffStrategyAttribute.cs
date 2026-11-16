using System;

namespace LogiCore.Plugins;

/// <summary>
/// Помечает тип тарифной стратегии как плагин, доступный для регистрации при старте приложения.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TariffStrategyAttribute : Attribute
{
    /// <summary>
    /// Создаёт атрибут для плагинной тарифной стратегии.
    /// </summary>
    public TariffStrategyAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя тарифной стратегии не может быть пустым.", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Отображаемое имя стратегии.
    /// </summary>
    public string Name { get; }
}
