using LogiCore.Domain.Services;

namespace LogiCore.App.Console.Notifications;

public sealed class ConsoleNotifier
{
    public void Subscribe(DeliveryService service)
    {
        service.OrderCreated += OnOrderCreated;
        service.OrderStatusChanged += OnOrderStatusChanged;
        service.VehicleOverloadAttempt += OnVehicleOverloadAttempt;
        service.DeliveryCompleted += OnDeliveryCompleted;
    }

    public void Unsubscribe(DeliveryService service)
    {
        service.OrderCreated -= OnOrderCreated;
        service.OrderStatusChanged -= OnOrderStatusChanged;
        service.VehicleOverloadAttempt -= OnVehicleOverloadAttempt;
        service.DeliveryCompleted -= OnDeliveryCompleted;
    }

    private static void OnOrderCreated(object? sender, OrderCreatedEventArgs e)
    {
        System.Console.WriteLine($"[Событие] Создан заказ для клиента «{e.Order.Customer.Name}».");
    }

    private static void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        System.Console.WriteLine(
            $"[Событие] Заказ {e.Order.Id}: {e.PreviousStatus} -> {e.CurrentStatus}.");
    }

    private static void OnVehicleOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e)
    {
        System.Console.WriteLine(
            $"[Событие] Транспорт {e.Vehicle.RegistrationNumber} не подходит по нагрузке/совместимости.");
    }

    private static void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e)
    {
        System.Console.WriteLine(
            $"[Событие] Доставка завершена; выручка: {e.Revenue:0.##}.");
    }
}
