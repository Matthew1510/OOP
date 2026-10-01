using System;

namespace LogiCore.App.Console.Commands;

public sealed class CancelOrderCommand : Command// отмена заказа
{
    public CancelOrderCommand() : base("cancel", "Отменить заказ"){}

    public override void Execute(ConsoleContext context){
        if (context.Orders.Count == 0){
            System.Console.WriteLine("Заказы пока не созданы.");
            return;
        }

        int orderNumber = ReadInt("Номер заказа: ", 1, context.Orders.Count);
        context.DeliveryService.CancelOrder(context.GetOrder(orderNumber));
        System.Console.WriteLine("Заказ отменён.");
    }
}
