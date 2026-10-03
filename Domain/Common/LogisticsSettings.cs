namespace LogiCore.Domain.Common;

public sealed class LogisticsSettings // настройки логистики
{
    private static readonly Lazy<LogisticsSettings> instance = new Lazy<LogisticsSettings>(() => new LogisticsSettings());

    private LogisticsSettings(){// приватный конструктор для реализации паттерна Singleton
        DefaultTollRoadCoefficient = 1.20m;
        DroneMaxFlightDistanceKm = 50m;
    }

    public static LogisticsSettings Instance => instance.Value;

    public decimal DefaultTollRoadCoefficient { get; set; }// коэффициент для расчета стоимости проезда по платным дорогам
    public decimal DroneMaxFlightDistanceKm { get; set; }// максимальная дальность полета дрона в километрах
}
