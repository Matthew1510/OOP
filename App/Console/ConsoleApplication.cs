using System;
using System.Collections.Generic;
using System.IO;
using LogiCore.App.Console.Commands;
using LogiCore.Domain;
using LogiCore.Domain.Factories;

namespace LogiCore.App.Console;

public sealed class ConsoleApplication{
    private readonly ConsoleContext context;
    private readonly List<IConsoleCommand> commands;

    public ConsoleApplication(){// инициализация приложения
        context = new ConsoleContext();// создание контекста
        AddInitialVehicles();
        commands = new List<IConsoleCommand>// команды 
        {
            new HelpCommand(new List<IConsoleCommand>()),
            new DemonstrateInterfacesCommand(),
            new CreateOrderCommand(),
            new ListOrdersCommand(),
            new ListVehiclesCommand(),
            new AssignTransportCommand(),
            new СhooseСheapTransportCommand(),
            new StartDeliveryCommand(),
            new CompleteDeliveryCommand(),
            new CancelOrderCommand(),
            new ExitCommand()
        };
        commands[0] = new HelpCommand(commands);
    }

    public void Run(string? commandFilePath = null){
        System.Console.WriteLine("Добро пожаловать в LogiCore CLI!");
        if (commandFilePath is not null)
            RunFromFile(commandFilePath);
        if (context.IsRunning){
            System.Console.WriteLine("Интерактивный режим. Введите help для списка команд.");
            RunCommands();
        }
    }

    private void RunFromFile(string filePath){
        TextReader originalInput = System.Console.In;
        using (StreamReader fileInput = new StreamReader(filePath)){
            System.Console.SetIn(fileInput);
            System.Console.WriteLine($"Запуск демо из файла: {filePath}");
            try{
                RunCommands();
            }
            finally{
                System.Console.SetIn(originalInput);
            }
        }
    }

    private void RunCommands(){
        while (context.IsRunning){
            System.Console.Write("> ");
            string? input = System.Console.ReadLine();
            if (input is null)
                break;
            if (string.IsNullOrWhiteSpace(input))
                continue;
            ExecuteCommand(input.Trim());
            //читаем строки - передаеем в строку в исполнитель команд
        }
    }

    private void ExecuteCommand(string commandName){ // исполнитель команды
        IConsoleCommand? selectedCommand = null;
        foreach (IConsoleCommand command in commands)
            if (string.Equals(command.Name, commandName, StringComparison.OrdinalIgnoreCase)){
                selectedCommand = command;
                break;
            }
        
        if (selectedCommand is null){
            System.Console.WriteLine("Неизвестная команда. Введите help.");
            return;
        }
        try{
            selectedCommand.Execute(context);// выполнение команды
        }
        catch (LogisticsException e) when (e is RouteNotFoundException){
            System.Console.WriteLine($"Маршрут не найден: {e.Message}");
        }
        catch (LogisticsException e) when (e is VehicleOverloadException){
            System.Console.WriteLine($"Не удалось подобрать транспорт: {e.Message}");
        }
        catch (LogisticsException e){
            System.Console.WriteLine($"Ошибка логистики: {e.Message}");
        }
        catch (ArgumentException e) when (e is not ArgumentNullException){
            System.Console.WriteLine($"Некорректные данные: {e.Message}");
        }
        catch (Exception e){
            System.Console.WriteLine($"Ошибка: {e.Message}");
        }
    }

    private void AddInitialVehicles(){// добавление транспорта
        AddVehicle(new TruckFactory().Create("TRUCK-001"));
        AddVehicle(new TruckFactory().Create("TRUCK-002"));
        AddVehicle(new RefrigeratorTruckFactory().Create("REF-001"));
        AddVehicle(new CargoPlaneFactory().Create("PLANE-001"));
        AddVehicle(new CargoShipFactory().Create("SHIP-001"));
        AddVehicle(new DroneCourierFactory().Create("DRONE-001"));
    }

    private void AddVehicle(Vehicle vehicle){// добавление транспорта в контекст
        context.Vehicles.Add(vehicle);
    }
}
