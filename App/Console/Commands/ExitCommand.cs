namespace LogiCore.App.Console.Commands;

public sealed class ExitCommand : Command
{
    public ExitCommand(): base("exit", "Выйти из приложения"){}

    public override void Execute(ConsoleContext context){
        context.IsRunning = false;
    }
}
