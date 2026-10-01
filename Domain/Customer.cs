using System;
using System.Collections.Generic;
using LogiCore.Domain.Common;

namespace LogiCore.Domain;

/// <summary>
/// Клиент логистической компании.
/// </summary>
public sealed class Customer : IEquatable<Customer>, IIdentifiable
{
    private readonly List<Order> orderHistory = new();

    public Customer(string name, string contact)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя клиента не может быть пустым.", nameof(name));
        if (string.IsNullOrWhiteSpace(contact))
            throw new ArgumentException("Контакт клиента не может быть пустым.", nameof(contact));
        Id = Guid.NewGuid();
        Name = name;
        Contact = contact;
    }

    /// <summary>
    /// Уникальный идентификатор клиента.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Имя клиента.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Контактная информация клиента.
    /// </summary>
    public string Contact { get; }

    /// <summary>
    /// История оформленных клиентом заказов.
    /// </summary>
    public IReadOnlyCollection<Order> OrderHistory { get {return orderHistory.AsReadOnly();} }
    internal void AddOrder(Order order){
        if (order is null)
            throw new ArgumentNullException(nameof(order), "Заказ не может быть null.");
        orderHistory.Add(order);
    }

    public bool Equals(Customer? other){
        return other is not null && Id == other.Id;
    }
    public override bool Equals(object? obj){
        return obj is Customer other && Equals(other);}
    public override int GetHashCode(){
        return Id.GetHashCode();
    }
}
