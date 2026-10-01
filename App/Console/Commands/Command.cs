using System;
using System.Collections.Generic;
using LogiCore.Domain.Decorators;

namespace LogiCore.App.Console.Commands;

public abstract class Command : IConsoleCommand
{
    protected Command(string name, string description) {
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; } // описание 
    public abstract void Execute(ConsoleContext context);// выполнение команды

    protected static string ReadRequired(string s){ // чтение строки с проверкой на пустоту
        while (true) {
            System.Console.Write(s);
            string? value = System.Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
            System.Console.WriteLine("Значение не может быть пустым.");
        }
    }

    protected static decimal ReadNumbers(string s){// чтение чисел
        while (true) {
            string value = ReadRequired(s);
            if (decimal.TryParse(value, out decimal result))
                return result;
            System.Console.WriteLine("Введите число.");
        }
    }

    protected static int ReadInt(string s, int min, int max)// чтение  числа с проверкой диапазона
    {
        while (true){
            string value = ReadRequired(s);
            if (int.TryParse(value, out int result) && result >= min && result <= max)
                return result;
            System.Console.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }

    protected static bool ReadYesNo(string prompt){
        while (true){
            string value = ReadRequired(prompt);
            if (string.Equals(value, "да", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "д", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "y", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(value, "нет", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "н", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "n", StringComparison.OrdinalIgnoreCase))
                return false;
            System.Console.WriteLine("Введите «да» или «нет».");
        }
    }

    protected static void PrintCostQuote(IReadOnlyList<IDeliveryCostItem> quote){
        foreach (IDeliveryCostItem step in quote)
            System.Console.WriteLine($"{step.Description}: {step.Total:0.##}");
    }
}
