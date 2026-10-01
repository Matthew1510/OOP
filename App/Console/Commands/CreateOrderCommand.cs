using System;
using System.Collections.Generic;
using LogiCore.Domain;
using LogiCore.Domain.Factories;

namespace LogiCore.App.Console.Commands;

public sealed class CreateOrderCommand : Command// создание заказа
{
    public CreateOrderCommand() : base("create", "Создать заказ"){}

    public override void Execute(ConsoleContext context){
        string customerName = ReadRequired("Имя клиента: ");
        string customerContact = ReadRequired("Контакт клиента: ");
        System.Console.WriteLine("Тип груза: 1 — обычный, 2 — скоропортящийся, 3 — хрупкий, 4 — опасный, 5 — негабаритный.");
        int cargoTypeNumber = ReadInt("Выберите тип груза: ", 1, 5);
        string cargoType = GetCargoType(cargoTypeNumber);
        string description = ReadRequired("Описание груза: ");
        decimal weight = ReadNumbers("Вес груза, кг: ");
        decimal volume = ReadNumbers("Объём груза, м3: ");
        decimal declaredValue = ReadNumbers("Объявленная стоимость: ");
        DateTime? expirationDate = null;
        decimal neededTemperature = 0m;
        int hazardClass = 1;
        int fragilityLevel = 1;
        decimal lengthM = 1m;
        decimal widthM = 1m;
        decimal heightM = 1m;

        if (cargoTypeNumber == 2){
            expirationDate = ReadDate("Срок годности (например, 31.12.2026): ");
            neededTemperature = ReadNumbers("Необходимая температура, °C: ");
        }
        if (cargoTypeNumber == 3)
            fragilityLevel = ReadInt("Уровень хрупкости (1–5): ", 1, 5);
        if (cargoTypeNumber == 4)
            hazardClass = ReadInt("Класс опасности (1–9): ", 1, 9);
        if (cargoTypeNumber == 5){
            lengthM = ReadNumbers("Длина, м: ");
            widthM = ReadNumbers("Ширина, м: ");
            heightM = ReadNumbers("Высота, м: ");
        }

        decimal endX = ReadNumbers("Конечная координата X: ");
        decimal endY = ReadNumbers("Конечная координата Y: ");
        bool insuranceSelected = ReadYesNo("Добавить страхование? (да/нет): ");
        bool prioritySelected = ReadYesNo("Добавить приоритетную доставку? (да/нет): ");

        Customer customer = new Customer(customerName, customerContact);
        Cargo cargo = CargoFactory.CreateCargo(
            cargoType,
            description,
            weight,
            volume,
            declaredValue,
            expirationDate,
            neededTemperature,
            hazardClass,
            fragilityLevel,
            lengthM,
            widthM,
            heightM);
        Route route = new Route(new[]{new RoutePoint(0m, 0m),new RoutePoint(endX, endY) });
        Order order = context.DeliveryService.CreateOrder(
            customer,
            new List<Cargo> { cargo },
            route,
            insuranceSelected,
            prioritySelected);

        context.Customers.Add(customer);
        context.Orders.Add(order);

        System.Console.WriteLine($"Заказ создан. Номер заказа: {context.Orders.Count}.");
    }

    private static string GetCargoType(int typeNumber){
        if (typeNumber == 1)
            return "StandardCargo";
        if (typeNumber == 2)
            return "PerishableCargo";
        if (typeNumber == 3)
            return "FragileCargo";
        if (typeNumber == 4)
            return "DangerousCargo";
        return "OversizedCargo";
    }

    private static DateTime ReadDate(string prompt){
        while (true){
            string value = ReadRequired(prompt);
            if (DateTime.TryParse(value, out DateTime result))
                return result;
            System.Console.WriteLine("Введите дату в корректном формате.");
        }
    }
}
