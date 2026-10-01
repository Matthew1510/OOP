using LogiCore.Domain.Common;

namespace LogiCore.Domain;

/**
 * Состояние заказа в жизненном цикле доставки.
 */
public enum OrderStatus
{
    /// <summary>
    /// Заказ создан и ещё не назначен транспорт.
    /// </summary>
    Created,

    /// <summary>
    /// Заказ назначен на транспорт, но доставка ещё не началась.
    /// </summary>
    Assigned,

    /// <summary>
    /// Доставка находится в процессе выполнения.
    /// </summary>
    InTransit,

    /// <summary>
    /// Доставка успешно завершена.
    /// </summary>
    Delivered,

    /// <summary>
    /// Заказ отменён до завершения доставки.
    /// </summary>
    Cancelled
}

/**
 * Представляет заказ на доставку груза.
 *
 * <para>
 * Заказ хранит клиента, набор грузов, маршрут и назначенный транспорт.
 * Изменение статуса допустимо только через методы конечного автомата.
 * </para>
 */
public class Order : IIdentifiable
{
    private readonly List<Cargo> cargo;

    /// <summary>
    /// Создаёт новый заказ.
    /// </summary>
    /// <param name="customer">Клиент, оформляющий заказ.</param>
    /// <param name="cargo">Список грузов, включённых в заказ.</param>
    /// <param name="route">Маршрут доставки.</param>
    /// <param name="assignedVehicle">Назначенный транспорт, если он известен заранее.</param>
    /// <param name="totalCost">Итоговая стоимость заказа.</param>
    /// <exception cref="ArgumentNullException">Когда клиент или список грузов равны null.</exception>
    /// <exception cref="RouteNotFoundException">Когда маршрут не задан.</exception>
    /// <exception cref="ArgumentException">Когда список грузов содержит null-элемент.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Когда стоимость заказа отрицательная.</exception>
    public Order(
        Customer customer,
        IEnumerable<Cargo> cargo,
        Route route,
        Vehicle? assignedVehicle = null,
        decimal totalCost = 0,
        bool insuranceSelected = false,
        bool prioritySelected = false) {

        if (customer is null)
            throw new ArgumentNullException(nameof(customer));
        if (route is null)
            throw new RouteNotFoundException("Для заказа не задан маршрут.");
        if (cargo is null)
            throw new ArgumentNullException(nameof(cargo));

        this.cargo = new List<Cargo>(cargo);

        foreach (Cargo? item in this.cargo){
            if (item is null)
                throw new ArgumentException("Список грузов не может содержать null.",nameof(cargo));
        }
        if (totalCost < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCost),"Стоимость не может быть отрицательной.");

        Customer = customer;// клиент
        Route = route; // маршрут
        Id = Guid.NewGuid();
        AssignedVehicle = assignedVehicle;// назначенный транспорт
        TotalCost = totalCost;// общая стоимость
        InsuranceSelected = insuranceSelected;
        PrioritySelected = prioritySelected;
        Status = OrderStatus.Created; // статус заказа
        Customer.AddOrder(this);
    }

    /// <summary>
    /// Уникальный идентификатор заказа.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Клиент, оформивший заказ.
    /// </summary>
    public Customer Customer { get; }

    /// <summary>
    /// Набор грузов, включённых в заказ.
    /// </summary>
    public IReadOnlyCollection<Cargo> Cargo { get { return cargo.AsReadOnly(); } }

    /// <summary>
    /// Маршрут доставки заказа.
    /// </summary>
    public Route Route { get; }

    /// <summary>
    /// Назначенный транспорт, если заказ уже привязан к машине.
    /// </summary>
    public Vehicle? AssignedVehicle { get; private set; }

    /// <summary>
    /// Итоговая стоимость доставки заказа.
    /// </summary>
    public decimal TotalCost { get; private set; }
    public bool InsuranceSelected { get; }
    public bool PrioritySelected { get; }

    /// <summary>
    /// Текущий статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Назначает заказ на указанное транспортное средство.
    /// </summary>
    /// <param name="vehicle">Транспорт, который будет выполнять доставку.</param>
    /// <exception cref="ArgumentNullException">Когда транспорт не передан.</exception>
    /// <exception cref="InvalidOrderStateException">Когда заказ не находится в состоянии <see cref="OrderStatus.Created"/>.</exception>
    public void Assign(Vehicle vehicle)
    {
        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));

        EnsureStatus(OrderStatus.Created);
        vehicle.Assign();
        AssignedVehicle = vehicle;
        Status = OrderStatus.Assigned;
    }

    /// <summary>
    /// Запускает доставку после назначения транспорта.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Когда заказ не находится в состоянии <see cref="OrderStatus.Assigned"/> или транспорт не назначен.</exception>
    public void StartDelivery()
    {
        EnsureStatus(OrderStatus.Assigned);
        if (AssignedVehicle is null)    
            throw new InvalidOrderStateException("Нельзя начать доставку без назначенного транспорта.");
        AssignedVehicle.StartDelivery();
        Status = OrderStatus.InTransit;
    }

    /// <summary>
    /// Завершает доставку и освобождает транспорт.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Когда заказ не находится в состоянии <see cref="OrderStatus.InTransit"/> или транспорт не назначен.</exception>
    public void Complete()
    {
        EnsureStatus(OrderStatus.InTransit);

        if (AssignedVehicle is null)
            throw new InvalidOrderStateException("Нельзя завершить доставку без назначенного транспорта.");

        AssignedVehicle.Release();
        Status = OrderStatus.Delivered;
    }

    /// <summary>
    /// Отменяет заказ, если он ещё не завершён.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Когда отмена заказа недопустима для текущего состояния.</exception>
    public void Cancel()
    {
        if (Status != OrderStatus.Created && Status != OrderStatus.Assigned)
            throw new InvalidOrderStateException($"Заказ в состоянии '{Status}' нельзя отменить.");
        if (Status == OrderStatus.Assigned && AssignedVehicle is not null)
            AssignedVehicle.Release();// освобождение транспорта при отмене заказа

        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Изменяет итоговую стоимость заказа до начала доставки.
    /// </summary>
    /// <param name="totalCost">Новая стоимость заказа.</param>
    /// <exception cref="ArgumentOutOfRangeException">Когда стоимость отрицательная.</exception>
    /// <exception cref="InvalidOrderStateException">Когда изменение стоимости недопустимо для текущего статуса.</exception>
    public void SetTotalCost(decimal totalCost)
    {
        if (totalCost < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCost),"Стоимость не может быть отрицательной.");
        if (Status != OrderStatus.Created && Status != OrderStatus.Assigned)
            throw new InvalidOrderStateException( "Стоимость можно изменить только до начала доставки.");

        TotalCost = totalCost;
    }

    /// <summary>
    /// Проверяет, соответствует ли текущий статус ожидаемому.
    /// </summary>
    /// <param name="expectedStatus">Ожидаемое состояние заказа.</param>
    /// <exception cref="InvalidOrderStateException">Когда текущее состояние отличается от ожидаемого.</exception>
    private void EnsureStatus(OrderStatus expectedStatus)
    {
        if (Status != expectedStatus)
            throw new InvalidOrderStateException($"Для операции требуется состояние '{expectedStatus}', текущее состояние: '{Status}'.");
    }
}