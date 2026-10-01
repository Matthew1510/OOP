using System;
using System.Collections.Generic;

namespace LogiCore.Domain;

/// <summary>
/// Точка маршрута с координатами на плоскости.
/// </summary>
public readonly struct RoutePoint
{
    /// <summary>
    /// Создаёт точку маршрута.
    /// </summary>
    public RoutePoint(decimal x, decimal y){
        X = x;
        Y = y;
    }

    /// <summary>
    /// Координата X точки.
    /// </summary>
    public decimal X { get; }

    /// <summary>
    /// Координата Y точки.
    /// </summary>
    public decimal Y { get; }

    /// <summary>
    /// Возвращает расстояние между двумя точками маршрута.
    /// </summary>
    public static decimal operator -(RoutePoint left, RoutePoint right)
    {
        decimal deltaX = left.X - right.X;
        decimal deltaY = left.Y - right.Y;

        return (decimal)Math.Sqrt((double)(deltaX* deltaX + deltaY*deltaY));
    }

    /// <summary>
    /// Выполняет явное преобразование точки в текстовое представление.
    /// </summary>
    public static explicit operator string(RoutePoint p){
        return p.ToString();
    }

    /// <inheritdoc />
    public override string ToString(){return $"({X:0.##}; {Y:0.##})";}
}

/// <summary>
/// Маршрут доставки, заданный последовательностью точек.
/// </summary>
public class Route
{
    /// <summary>
    /// Создаёт маршрут на основе последовательности точек.
    /// </summary>
    public Route(IEnumerable<RoutePoint> points){
        if (points is null)
            throw new ArgumentNullException(nameof(points), "Маршрут не может быть пустым.");
        Points = new List<RoutePoint>(points);
        if (Points.Count == 0)
            throw new ArgumentException("Маршрут должен содержать хотя бы одну точку.", nameof(points));
    }

    /// <summary>
    /// Точки маршрута в порядке следования.
    /// </summary>
    public IReadOnlyList<RoutePoint> Points { get; }

    /// <summary>
    /// Полная длина маршрута в километрах.
    /// </summary>
    public decimal DistanceKm{
        get{
            decimal distance = 0;
            for (int index = 1; index < Points.Count; index++)
                distance += Points[index] - Points[index - 1];
            return distance;
        }
    }

    /// <summary>
    /// Прямое расстояние между первой и последней точкой маршрута.
    /// </summary>
    public decimal DirectDistanceKm
    {
        get{
            if (Points.Count < 2)
                return 0m;
            return Points[Points.Count - 1] - Points[0];
        }
    }

    /// <summary>
    /// Оценивает время доставки для указанного транспортного средства.
    /// </summary>
    public TimeSpan EstimateTime(Vehicle vehicle)
    {
        if (vehicle is null)
            throw new ArgumentNullException(nameof(vehicle));
        return TimeSpan.FromHours((double)(DistanceKm / vehicle.AverageSpeedKmH));
    }
}
