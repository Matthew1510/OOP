using System;
using System.Linq;
using LogiCore.Domain;

namespace LogiCore.App.Console.Commands;

public sealed class ListOrdersCommand : Command// показать заказы
{
    public ListOrdersCommand() : base("orders", "Показать заказы"){}

    public override void Execute(ConsoleContext c){
        if (c.Orders.Count == 0){
            System.Console.WriteLine("Заказы пока не созданы.");
            return;
        }

        int index = 0;
        foreach (Order order in c.Orders) {
            index++;
            string vehicle = "не назначен";
            if (order.AssignedVehicle is not null)
                vehicle = order.AssignedVehicle.RegistrationNumber;

            string cargoTypes = string.Join(", ", order.Cargo.Select(cargo => cargo.GetType().Name));
            System.Console.WriteLine($"{index}. {order.Customer.Name} | Груз: {cargoTypes} | Статус: {order.Status} | Транспорт: {vehicle} | Стоимость: {order.TotalCost:0.##}");
        }
    }
}
