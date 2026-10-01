using System;
using System.IO;
using LogiCore.Domain.Services;

namespace LogiCore.App.Console.Notifications;

public sealed class EventFileLogger
{
    private readonly string filePath;

    public EventFileLogger(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к журналу не может быть пустым.", nameof(filePath));

        this.filePath = filePath;
    }

    public string FilePath => filePath;

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

    private void OnOrderCreated(object? sender, OrderCreatedEventArgs e)
    {
        Write($"Создан заказ {e.Order.Id} для клиента «{e.Order.Customer.Name}».");
    }

    private void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        Write($"Заказ {e.Order.Id}: {e.PreviousStatus} -> {e.CurrentStatus}.");
    }

    private void OnVehicleOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e)
    {
        Write(
            $"Транспорт {e.Vehicle.RegistrationNumber} не подошёл для груза: " +
            string.Join(", ", e.Cargo.Select(cargo => cargo.Description)) + ".");
    }

    private void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e)
    {
        Write($"Доставка заказа {e.Order.Id} завершена; выручка {e.Revenue:0.##}.");
    }

    private void Write(string message)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.AppendAllText(filePath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
    }
}
