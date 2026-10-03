using System;

namespace LogiCore.Domain.Decorators;

/// <summary>
/// Элемент стоимости доставки, участвующий в цепочке декораторов.
/// </summary>
public interface IDeliveryCostItem// Component
{
    /// <summary>
    /// Текстовое описание стоимости.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Итоговая стоимость.
    /// </summary>
    decimal Total { get; }
}

/// <summary>
/// Конкретный элемент стоимости доставки.
/// </summary>
public class DeliveryCostItem : IDeliveryCostItem// ConcreteComponent
{
    public DeliveryCostItem(decimal total, string description){
        if (total < 0m)
            throw new ArgumentOutOfRangeException(nameof(total));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание стоимости не может быть пустым.", nameof(description));

        Total = total;
        Description = description;
    }

    /// <summary>
    /// Описание стоимости.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Итоговая стоимость.
    /// </summary>
    public decimal Total { get; }
}

/// <summary>
/// Базовый декоратор для изменения стоимости доставки.
/// </summary>
public abstract class DeliveryCostDecorator : IDeliveryCostItem// Decorator
{
    /// <summary>
    /// Создаёт декоратор поверх другого элемента стоимости.
    /// </summary>
    protected DeliveryCostDecorator(IDeliveryCostItem item){
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }

    /// <summary>
    /// Внутренний элемент, над которым применяется декоратор.
    /// </summary>
    protected IDeliveryCostItem Item { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public abstract decimal Total { get; }
}

/// <summary>
/// Декоратор, добавляющий страхование к стоимости доставки.
/// </summary>
public sealed class InsuranceDecorator : DeliveryCostDecorator// ConcreteDecorator
{
    private readonly decimal declaredValue;
    private readonly decimal rate;

    public InsuranceDecorator(IDeliveryCostItem item, decimal declaredValue, decimal rate): base(item){
        if (declaredValue < 0m)
            throw new ArgumentOutOfRangeException(nameof(declaredValue));
        if (rate < 0m)
            throw new ArgumentOutOfRangeException(nameof(rate));
        this.declaredValue = declaredValue;
        this.rate = rate;
    }

    public override string Description{
        get{return Item.Description + ", страхование";}
    }

    public override decimal Total
    {
        get { return Item.Total + declaredValue * rate; }
    }
}

/**
 * Декоратор, увеличивающий стоимость с учётом срочности доставки.
 */
public sealed class PriorityDecorator : DeliveryCostDecorator// ConcreteDecorator
{
    private readonly decimal coefficient;

    public PriorityDecorator(IDeliveryCostItem item, decimal coefficient): base(item){
        if (coefficient < 1m)
            throw new ArgumentOutOfRangeException(nameof(coefficient));
        this.coefficient = coefficient;
    }

    public override string Description{
        get{return Item.Description + ", приоритетная доставка";}
    }

    public override decimal Total {
        get{return Item.Total * coefficient;}
    }
}
