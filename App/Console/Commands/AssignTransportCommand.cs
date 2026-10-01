using System;
using LogiCore.Domain;

namespace LogiCore.App.Console.Commands;

public sealed class AssignTransportCommand : Command//назначение транспорта
{
    public AssignTransportCommand(): base("assign", "Назначить транспорт заказу"){}

    public override void Execute(ConsoleContext context){
        if (context.Orders.Count == 0 || context.Vehicles.Count == 0){
            System.Console.WriteLine("Сначала создайте заказ и добавьте транспорт.");
            return;
        }

        int orderNumber = ReadInt("Номер заказа: ", 1, context.Orders.Count);
        int vehicleNumber = ReadInt("Номер транспорта: ", 1, context.Vehicles.Count);

        Order order = context.GetOrder(orderNumber);
        Vehicle vehicle = context.GetVehicle(vehicleNumber);
        context.DeliveryService.AssignVehicle(order, vehicle);
        PrintCostQuote(context.DeliveryService.GetDeliveryCostQuote(order, vehicle));
        System.Console.WriteLine($"Транспорт назначен. Стоимость: {order.TotalCost:0.##}.");
    }
}
