using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LogiCore.Domain.Strategies;

namespace LogiCore.Plugins;

/// <summary>
/// Загружает тарифные стратегии из внешней папки плагинов.
/// </summary>
public static class TariffStrategyLoader
{
    /// <summary>
    /// Возвращает все зарегистрированные внешние тарифные стратегии из директории плагинов.
    /// </summary>
    public static IReadOnlyList<ITariffStrategy> Load(string pluginDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory))
            throw new ArgumentException("Путь к директории плагинов не может быть пустым.", nameof(pluginDirectory));

        List<ITariffStrategy> strategies = new List<ITariffStrategy>();
        if (!Directory.Exists(pluginDirectory))
            return strategies;

        foreach (string filePath in Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(filePath);
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!typeof(ITariffStrategy).IsAssignableFrom(type))
                        continue;
                    if (type.GetCustomAttribute<TariffStrategyAttribute>() is null)
                        continue;

                    if (Activator.CreateInstance(type) is ITariffStrategy strategy)
                        strategies.Add(strategy);
                }
            }
            catch (FileLoadException)
            {
                // Плагин уже подгружен или библиотека повреждена; пропускаем.
            }
            catch (ReflectionTypeLoadException)
            {
                // Внешняя библиотека содержит типы, недоступные в текущем контексте.
            }
        }

        return strategies.AsReadOnly();
    }
}
