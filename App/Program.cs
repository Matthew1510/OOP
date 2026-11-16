using LogiCore.App.Console;
using LogiCore.Domain.Strategies;
using LogiCore.Plugins;

System.Console.WriteLine("Добро пожаловать в LogiCore!");
string pluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
IReadOnlyList<ITariffStrategy> pluginStrategies = TariffStrategyLoader.Load(pluginDirectory);

if (pluginStrategies.Count > 0)
{
    System.Console.WriteLine($"Загружено внешних тарифных плагинов: {pluginStrategies.Count}");
    foreach (ITariffStrategy strategy in pluginStrategies)
        System.Console.WriteLine($"  - {strategy.Name}");
}

ConsoleApplication application = new ConsoleApplication(pluginStrategies);
string? commandFilePath = null;
if (args.Length > 0)
    commandFilePath = args[0];
application.Run(commandFilePath);
// точка входа