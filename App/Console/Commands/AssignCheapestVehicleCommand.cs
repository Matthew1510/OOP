using System;
using LogiCore.Domain;

namespace LogiCore.App.Console.Commands;

public sealed class СhooseСheapTransportCommand : Command
{  
    // выбор транспорта
    public СhooseСheapTransportCommand(): base("autochoose", "Автоматически выбрать самый дешёвый транспорт"){}
    public override void Execute(ConsoleContext context)
    {
        if (context.Orders.Count == 0){// проверка на наличие заказов
            System.Console.WriteLine("Заказы пока не созданы.");
            return;
        }

        int orderNumber = ReadInt("Номер заказа: ", 1, context.Orders.Count);
        Order order = context.GetOrder(orderNumber);
        Vehicle vehicle = context.DeliveryService.AssignCheapestVehicle(order);

        PrintCostQuote(context.DeliveryService.GetDeliveryCostQuote(order, vehicle));
        System.Console.WriteLine($"Выбран транспорт: {vehicle.RegistrationNumber}. Стоимость: {order.TotalCost:0.##}.");
    }
}
