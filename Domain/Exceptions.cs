using System;

namespace LogiCore.Domain;

/// <summary>
/// Базовое доменное исключение системы логистики.
/// </summary>
public class LogisticsException : Exception
{
    /// <summary>
    /// Создаёт логистическое исключение.
    /// </summary>
    public LogisticsException(string message): base(message){}
}

/// <summary>
/// Исключение, возникающее при попытке выполнить недопустимый переход состояния заказа.
/// </summary>
public sealed class InvalidOrderStateException : LogisticsException
{
    /// <summary>
    /// Создаёт исключение для неправильного состояния заказа.
    /// </summary>
    public InvalidOrderStateException(string message) : base(message) { }
}

/// <summary>
/// Исключение, возникающее, когда для операции не найден или не задан маршрут.
/// </summary>
public sealed class RouteNotFoundException : LogisticsException
{
    /// <summary>
    /// Создаёт исключение для отсутствующего маршрута.
    /// </summary>
    public RouteNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Исключение, возникающее при ошибке валидации груза.
/// </summary>
public sealed class CargoValidationException : LogisticsException
{
    public CargoValidationException(string message): base(message){}
}

/// <summary>
/// Исключение, возникающее при несовместимости грузов в одной доставке.
/// </summary>
public sealed class IncompatibleCargoException : LogisticsException
{
    public IncompatibleCargoException(string message)  : base(message){}
}

/// <summary>
/// Исключение, возникающее при попытке перегруза транспортного средства.
/// </summary>
public sealed class VehicleOverloadException : LogisticsException
{
    public VehicleOverloadException(string message) : base(message){}
}
