using System;
using LogiCore.Domain;

namespace LogiCore.App.Console.Commands;

public sealed class ListVehiclesCommand : Command// показать доступный транспорт
{
    public ListVehiclesCommand() : base("vehicles", "Показать доступный транспорт"){}
    public override void Execute(ConsoleContext context){
        if (context.Vehicles.Count == 0){
            System.Console.WriteLine("Транспорт пока не добавлен.");
            return;
        }

        int index = 0;
        foreach (Vehicle vehicle in context.Vehicles){
            index++;
            System.Console.WriteLine($"{index}. {vehicle} | Состояние: {vehicle.State}");
        }
    }
}
