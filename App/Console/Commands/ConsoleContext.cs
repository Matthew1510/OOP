using LogiCore.Domain;
using LogiCore.Domain.Collections;
using LogiCore.Domain.Services;
using LogiCore.App.Console.Notifications;

namespace LogiCore.App.Console.Commands;

public sealed class ConsoleContext// контекст консоли
{
    public ConsoleContext(){
        Customers = new Repository<Customer>();// список клиентов
        Vehicles = new Repository<Vehicle>();// список транспорта
        Orders = new Repository<Order>();// список заказов
        DeliveryService = new DeliveryService(Vehicles);// сервис доставки
        ConsoleNotifier = new ConsoleNotifier();
        ConsoleNotifier.Subscribe(DeliveryService);
        EventFileLogger = new EventFileLogger(
            System.IO.Path.Combine(AppContext.BaseDirectory, "logs", "events.log"));
        EventFileLogger.Subscribe(DeliveryService);
        IsRunning = true;// флаг работы консоли
    }

    public Repository<Customer> Customers { get; }
    public Repository<Vehicle> Vehicles { get; }
    public Repository<Order> Orders { get; }
    public DeliveryService DeliveryService { get; }
    public ConsoleNotifier ConsoleNotifier { get; }
    public EventFileLogger EventFileLogger { get; }
    public bool IsRunning { get; set; }

    public Order GetOrder(int number){// получение заказа по номеру
        if (number < 1 || number > Orders.Count)
            throw new ArgumentOutOfRangeException(nameof(number), number, "Заказ не найден.");
        return Orders.GetAt(number - 1);
    }

    public Vehicle GetVehicle(int number){// получение транспорта по номеру
        if (number < 1 || number > Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(number), number, "Транспорт не найден.");
        return Vehicles.GetAt(number - 1);
    }
}
