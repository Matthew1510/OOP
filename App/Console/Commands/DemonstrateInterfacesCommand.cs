using System;
using LogiCore.Domain;
using LogiCore.Domain.Common;
using LogiCore.Domain.Services;

namespace LogiCore.App.Console.Commands;

public sealed class DemonstrateInterfacesCommand : Command


// нужна только для демо 


{
    public DemonstrateInterfacesCommand()
        : base("interfaces", "Показать вариантность интерфейсов (T3)")
    {
    }

    public override void Execute(ConsoleContext context){
        IReadOnlyRepository<Customer> customers = context.Customers;
        IReadOnlyRepository<IIdentifiable> identifiableItems = customers;
        System.Console.WriteLine(
            $"Ковариантность IReadOnlyRepository: доступно клиентов — {identifiableItems.GetAll().Count()}.");

        CargoCompatibilityValidator cargoValidator = new CargoCompatibilityValidator();
        IValidator<PerishableCargo> perishableCargoValidator = cargoValidator;
        PerishableCargo cargo = new PerishableCargo(
            "Демо скоропортящийся груз",
            1m,
            1m,
            0m,
            DateTime.Today.AddDays(1),
            4m);
        perishableCargoValidator.Validate(cargo).ThrowIfInvalid();
        System.Console.WriteLine("Контравариантность IValidator: валидатор для Cargo проверил PerishableCargo.");
    }
}
