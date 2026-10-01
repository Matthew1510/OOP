using System;

namespace LogiCore.App.Console.Commands;

public sealed class StartDeliveryCommand : Command// начать доставку
{
    public StartDeliveryCommand() : base("start", "Начать доставку заказа"){}

    public override void Execute(ConsoleContext context){
        if (context.Orders.Count == 0){
            System.Console.WriteLine("Заказы пока не созданы.");
            return;
        }

        int orderNumber = ReadInt("Номер заказа: ", 1, context.Orders.Count);
        context.DeliveryService.StartDelivery(context.GetOrder(orderNumber));
        System.Console.WriteLine("Доставка начата.");
    }
}