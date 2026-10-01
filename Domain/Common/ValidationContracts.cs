using System;
using System.Collections.Generic;

namespace LogiCore.Domain.Common;

/// <summary>
/// Контракт объекта, имеющего собственный идентификатор.
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// Уникальный идентификатор сущности.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Контракт валидатора, который проверяет корректность входного значения.
/// </summary>
public interface IValidator<in T>
{
    /// <summary>
    /// Выполняет проверку входного значения.
    /// </summary>
    ValidationResult Validate(T value);
}

/// <summary>
/// Контракт только для чтения репозитория сущностей.
/// </summary>
public interface IReadOnlyRepository<out T> where T : class, IIdentifiable
{
    /// <summary>
    /// Возвращает сущность по идентификатору.
    /// </summary>
    T? GetById(Guid id);

    /// <summary>
    /// Возвращает все сущности из репозитория.
    /// </summary>
    IEnumerable<T> GetAll();
}

/// <summary>
/// Результат валидации элемента.
/// </summary>
public sealed class ValidationResult
{
    private ValidationResult(bool isValid, Exception? error)
    {
        IsValid = isValid;
        Error = error;
    }

    /// <summary>
    /// Показывает, прошла ли валидация успешно.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Возвращает исключение, связанное с невалидным результатом.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Создаёт успешный результат проверки.
    /// </summary>
    public static ValidationResult Valid()
    {
        return new ValidationResult(true, null);
    }

    /// <summary>
    /// Создаёт результат проверки с ошибкой.
    /// </summary>
    public static ValidationResult Invalid(Exception error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error));

        return new ValidationResult(false, error);
    }

    /// <summary>
    /// Выбрасывает исключение, если валидация не прошла.
    /// </summary>
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw Error ?? new InvalidOperationException("Валидация завершилась ошибкой.");
    }
}
