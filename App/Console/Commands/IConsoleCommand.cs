namespace LogiCore.App.Console.Commands;

public interface IConsoleCommand //интерфейс для команд консоли
{
    string Name { get; }
    string Description { get; }
    void Execute(ConsoleContext context);
}
