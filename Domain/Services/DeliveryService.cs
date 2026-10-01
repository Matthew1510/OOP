using System;
using System.Collections.Generic;
using LogiCore.Domain.Collections;
using LogiCore.Domain.Common;
using LogiCore.Domain.Decorators;
using LogiCore.Domain.Strategies;

namespace LogiCore.Domain.Services;

/// <summary>
/// Валидатор, проверяющий совместимость грузов и соответствие правилам перевозки.
/// </summary>
public sealed class CargoCompatibilityValidator :
    IValidator<Cargo>,
    IValidator<IReadOnlyCollection<Cargo>>
{
    /// <summary>
    /// Проверяет один груз на допустимость перевозки.
    /// </summary>
    public ValidationResult Validate(Cargo cargo){
        if (cargo is null)
            return ValidationResult.Invalid(new ArgumentNullException(nameof(cargo)));

        return Validate(new List<Cargo> { cargo });
    }

    /// <summary>
    /// Проверяет набор грузов на совместимость и срок годности.
    /// </summary>
    public ValidationResult Validate(IReadOnlyCollection<Cargo> cargo){

        if (cargo is null)
            return ValidationResult.Invalid(new ArgumentNullException(nameof(cargo)));

        bool hasDangerousCargo = false;// проверка на наличие опасных грузов
        bool hasPerishableCargo = false;// проверка на наличие скоропортящихся грузов

        foreach (Cargo item in cargo){
            if (item is null)
                return ValidationResult.Invalid(new CargoValidationException("Список грузов содержит null."));
            if (item.IsExpired())
                return ValidationResult.Invalid(new CargoValidationException(
                    $"Срок годности груза '{item.Description}' истёк."));
            if (item is DangerousCargo)
                hasDangerousCargo = true;
            if (item is PerishableCargo)
                hasPerishableCargo = true;
        }
        if (hasDangerousCargo && hasPerishableCargo)
            return ValidationResult.Invalid(new IncompatibleCargoException(
                "Опасные и скоропортящиеся грузы нельзя перевозить вместе."));

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Проверяет совместимость набора грузов с конкретным транспортным средством.
    /// </summary>
    public void ValidateForVehicle(Vehicle vehicle, IReadOnlyCollection<Cargo> cargo){
        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));

        Validate(cargo).ThrowIfInvalid();

        decimal totalWeight = 0m;
        decimal totalVolume = 0m;

        foreach (Cargo item in cargo){
            if (!vehicle.CanCarry(item))
                throw new InvalidOperationException($"Транспорт '{vehicle.RegistrationNumber}' не подходит для груза '{item.Description}'.");

            totalWeight += item.WeightKg;
            totalVolume += item.VolumeM3;
        }

        if (totalWeight > vehicle.MaxLoadKg || totalVolume > vehicle.MaxVolumeM3)
            throw new VehicleOverloadException($"Транспорт '{vehicle.RegistrationNumber}' перегружен.");
    }
}

/**
 * Аргументы события создания заказа.
 */
public sealed class OrderCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы события.
    /// </summary>
    public OrderCreatedEventArgs(Order order)
    {
        Order = order;
    }

    /// <summary>
    /// Созданный заказ.
    /// </summary>
    public Order Order { get; }
}

/// <summary>
/// Аргументы события изменения статуса заказа.
/// </summary>
public sealed class OrderStatusChangedEventArgs : EventArgs
{
    public OrderStatusChangedEventArgs(Order order, OrderStatus previousStatus,OrderStatus currentStatus){
        Order = order;
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
    }

    /// <summary>
    /// Заказ, у которого изменился статус.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Предыдущий статус заказа.
    /// </summary>
    public OrderStatus PreviousStatus { get; }

    /// <summary>
    /// Текущий статус заказа.
    /// </summary>
    public OrderStatus CurrentStatus { get; }
}

/// <summary>
/// Аргументы события попытки перегруза транспорта.
/// </summary>
public sealed class VehicleOverloadAttemptEventArgs : EventArgs
{
    /// <summary>
    /// Создаёт аргументы события перегруза.
    /// </summary>
    public VehicleOverloadAttemptEventArgs(Vehicle vehicle, IReadOnlyCollection<Cargo> cargo){
        Vehicle = vehicle;
        Cargo = cargo;
    }

    /// <summary>
    /// Транспорт, который не смог принять груз.
    /// </summary>
    public Vehicle Vehicle { get; }

    /// <summary>
    /// Набор грузов, вызвавших перегруз или несовместимость.
    /// </summary>
    public IReadOnlyCollection<Cargo> Cargo { get; }
}

/// <summary>
/// Аргументы события завершения доставки.
/// </summary>
public sealed class DeliveryCompletedEventArgs : EventArgs
{
    public DeliveryCompletedEventArgs(Order order, decimal revenue){
        Order = order;
        Revenue = revenue;
    }

    /// <summary>
    /// Заказ, доставка которого завершена.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Выручка, полученная от выполнения этой доставки.
    /// </summary>
    public decimal Revenue { get; }
}

/// <summary>
/// Делегат для событий логистики.
/// </summary>
public delegate void LogisticsEventHandler(object sender, EventArgs eventArgs);

/// <summary>
/// Сервис диспетчеризации и управления доставками.
/// </summary>
public sealed class DeliveryService
{
    private const decimal InsuranceRate = 0.02m;
    private const decimal PriorityCoefficient = 1.20m;
    private readonly CargoCompatibilityValidator validator;
    private readonly Repository<Vehicle> vehicles;
    private readonly ITariffStrategy standardTariff;
    private readonly ITariffStrategy heavyCargoTariff;

    /// <summary>
    /// Создаёт сервис с пустым парком транспортных средств.
    /// </summary>
    public DeliveryService()
        : this(new Repository<Vehicle>())
    {
    }

    /// <summary>
    /// Создаёт сервис с набором транспортных средств.
    /// </summary>
    public DeliveryService(
        IEnumerable<Vehicle> vehicles,
        CargoCompatibilityValidator? validator = null)
        : this(CreateVehicleRepository(vehicles), validator)
    {
    }

    /// <summary>
    /// Создаёт сервис на основе существующего хранилища транспорта.
    /// </summary>
    public DeliveryService(
        Repository<Vehicle> vehicles,
        CargoCompatibilityValidator? validator = null)
    {
        if (vehicles is null)
            throw new ArgumentNullException(nameof(vehicles));

        this.vehicles = vehicles;
        this.validator = validator ?? new CargoCompatibilityValidator();
        standardTariff = new StandardTariff();
        heavyCargoTariff = new HeavyCargoTariff();
    }

    /// <summary>
    /// Добавляет транспорт в парк сервиса.
    /// </summary>
    public void AddVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));
        vehicles.Add(vehicle);
    }

    /// <summary>
    /// Общая выручка компании по завершённым доставкам.
    /// </summary>
    public decimal Revenue { get; private set; }

    /// <summary>
    /// Событие создания заказа.
    /// </summary>
    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;

    /// <summary>
    /// Событие изменения статуса заказа.
    /// </summary>
    public event EventHandler<OrderStatusChangedEventArgs>? OrderStatusChanged;

    /// <summary>
    /// Событие попытки перегруза транспортного средства.
    /// </summary>
    public event EventHandler<VehicleOverloadAttemptEventArgs>? VehicleOverloadAttempt;

    /// <summary>
    /// Событие завершения доставки.
    /// </summary>
    public event EventHandler<DeliveryCompletedEventArgs>? DeliveryCompleted;

    /// <summary>
    /// Создаёт новый заказ и связывает его с клиентом, грузами и маршрутом.
    /// </summary>
    public Order CreateOrder(
        Customer customer,
        IReadOnlyCollection<Cargo> cargo,
        Route route,
        bool insuranceSelected = false,
        bool prioritySelected = false)
    {
        validator.Validate(cargo).ThrowIfInvalid();
        Order order = new Order(
            customer,
            cargo,
            route,
            insuranceSelected: insuranceSelected,
            prioritySelected: prioritySelected);
        RaiseOrderCreated(order);
        return order;
    }

    /// <summary>
    /// Рассчитывает стоимость с применением тарифа и выбранных дополнительных услуг.
    /// </summary>
    public IReadOnlyList<IDeliveryCostItem> GetDeliveryCostQuote(Order order, Vehicle vehicle)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));
        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));

        decimal vehicleCost = vehicle.CalculateDeliveryCost(order.Route, order.Cargo);
        ITariffStrategy strategy = SelectTariff(order.Cargo);
        decimal tariffCost = strategy.Calculate(vehicleCost, order.Route, order.Cargo);
        List<IDeliveryCostItem> steps = new List<IDeliveryCostItem>
        {
            new DeliveryCostItem(vehicleCost, "Базовая стоимость транспорта"),
            new DeliveryCostItem(tariffCost, strategy.Name)
        };

        IDeliveryCostItem total = steps[steps.Count - 1];
        if (order.InsuranceSelected)
        {
            decimal declaredValue = CalculateDeclaredValue(order.Cargo);
            total = new InsuranceDecorator(total, declaredValue, InsuranceRate);
            steps.Add(total);
        }
        if (order.PrioritySelected)
        {
            total = new PriorityDecorator(total, PriorityCoefficient);
            steps.Add(total);
        }

        return steps.AsReadOnly();
    }

    /// <summary>
    /// Подбирает наиболее дешёвый подходящий транспорт для заказа.
    /// </summary>
    public Vehicle AssignCheapestVehicle(Order order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        Vehicle? cheapestVehicle = null;
        decimal cheapestCost = decimal.MaxValue;

        foreach (Vehicle vehicle in vehicles)
        {
            try
            {
                validator.ValidateForVehicle(vehicle, order.Cargo);
                IReadOnlyList<IDeliveryCostItem> quote = GetDeliveryCostQuote(order, vehicle);
                decimal cost = quote[quote.Count - 1].Total;

                if (cost < cheapestCost)
                {
                    cheapestCost = cost;
                    cheapestVehicle = vehicle;
                }
            }
            catch (VehicleOverloadException)
            {
                RaiseVehicleOverloadAttempt(vehicle, order.Cargo);
            }
            catch (InvalidOperationException)
            {
                RaiseVehicleOverloadAttempt(vehicle, order.Cargo);
            }
        }

        if (cheapestVehicle is null)
            throw new VehicleOverloadException(
                "Не найден подходящий свободный транспорт.");

        OrderStatus previousStatus = order.Status;
        order.Assign(cheapestVehicle);
        order.SetTotalCost(cheapestCost);
        RaiseOrderStatusChanged(order, previousStatus);
        return cheapestVehicle;
    }

    /// <summary>
    /// Назначает заказ на конкретный транспорт.
    /// </summary>
    public void AssignVehicle(Order order, Vehicle vehicle){
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));
        try{
            validator.ValidateForVehicle(vehicle, order.Cargo);
            IReadOnlyList<IDeliveryCostItem> quote = GetDeliveryCostQuote(order, vehicle);
            decimal cost = quote[quote.Count - 1].Total;
            OrderStatus previousStatus = order.Status;
            order.Assign(vehicle);
            order.SetTotalCost(cost);
            RaiseOrderStatusChanged(order, previousStatus);
        }
        catch (VehicleOverloadException){
            RaiseVehicleOverloadAttempt(vehicle, order.Cargo);
            throw;
        }
    }

    /// <summary>
    /// Запускает доставку для уже назначенного заказа.
    /// </summary>
    public void StartDelivery(Order order){
        if (order is null)
            throw new ArgumentNullException(nameof(order));


        OrderStatus previousStatus = order.Status;
        order.StartDelivery();
        RaiseOrderStatusChanged(order, previousStatus);
    }

    /// <summary>
    /// Завершает доставку и увеличивает выручку компании.
    /// </summary>
    public void CompleteDelivery(Order order){
        if (order is null)
            throw new ArgumentNullException(nameof(order));


        OrderStatus previousStatus = order.Status;
        order.Complete();
        Revenue += order.TotalCost;
        RaiseOrderStatusChanged(order, previousStatus);
        RaiseDeliveryCompleted(order);
    }

    /// <summary>
    /// Отменяет заказ и меняет его статус на отменённый.
    /// </summary>
    public void CancelOrder(Order order){
        if (order is null)
            throw new ArgumentNullException(nameof(order));
        OrderStatus previousStatus = order.Status;
        order.Cancel();
        RaiseOrderStatusChanged(order, previousStatus);
    }

    private void RaiseOrderCreated(Order order){// событие создания заказа
        EventHandler<OrderCreatedEventArgs>? handler = OrderCreated;
        if (handler is not null)
            handler(this, new OrderCreatedEventArgs(order));
    }

    private void RaiseOrderStatusChanged(Order order, OrderStatus previousStatus){// событие изменения статуса заказа
        EventHandler<OrderStatusChangedEventArgs>? handler = OrderStatusChanged;
        if (handler is not null)
            handler(this,new OrderStatusChangedEventArgs(order, previousStatus, order.Status));
    }

    private void RaiseVehicleOverloadAttempt(// событие попытки перегруза транспорта
        Vehicle vehicle,
        IReadOnlyCollection<Cargo> cargo)
    {
        if (VehicleOverloadAttempt != null)
            VehicleOverloadAttempt(this, new VehicleOverloadAttemptEventArgs(vehicle, cargo));
    }

    private void RaiseDeliveryCompleted(Order order){// событие завершения доставки
        EventHandler<DeliveryCompletedEventArgs>? handler = DeliveryCompleted;
        if (handler is not null)
            handler(this, new DeliveryCompletedEventArgs(order, order.TotalCost));
    }

    private static Repository<Vehicle> CreateVehicleRepository(IEnumerable<Vehicle> vehicles){// создание репозитория транспортных средств
        if (vehicles is null)
            throw new ArgumentNullException(nameof(vehicles));

        Repository<Vehicle> repository = new Repository<Vehicle>();
        foreach (Vehicle vehicle in vehicles)
            repository.Add(vehicle);
        return repository;
    }

    private ITariffStrategy SelectTariff(IReadOnlyCollection<Cargo> cargo){// выбор стратегии тарифа в зависимости от состава груза
        decimal totalWeight = 0m;
        foreach (Cargo item in cargo)
            totalWeight += item.WeightKg;

        if (totalWeight > 1000m)
            return heavyCargoTariff;
        return standardTariff;
    }

    private static decimal CalculateDeclaredValue(IReadOnlyCollection<Cargo> cargo){// расчет объявленной стоимости груза
        decimal total = 0m;
        foreach (Cargo item in cargo)
            total += ((IInsurable)item).GetInsuranceValue();
        return total;
    }
}
