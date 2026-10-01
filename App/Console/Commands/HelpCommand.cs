using System;
using System.Collections.Generic;

namespace LogiCore.App.Console.Commands;

public sealed class HelpCommand : Command
{
    private readonly IReadOnlyList<IConsoleCommand> commands;

    public HelpCommand(IReadOnlyList<IConsoleCommand> commands) : base("help", "Показать список команд")
        {this.commands = commands;}

    public override void Execute(ConsoleContext context){
        foreach (IConsoleCommand command in commands)
            System.Console.WriteLine($"{command.Name,-10} {command.Description}");
    }
}
