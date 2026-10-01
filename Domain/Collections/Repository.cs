using System.Collections;
using LogiCore.Domain.Common;

namespace LogiCore.Domain.Collections;

/// <summary>
/// Собственное хранилище сущностей в памяти с доступом по идентификатору и перечислением.
/// </summary>
public sealed class Repository<T> : IReadOnlyRepository<T>, IEnumerable<T> where T : class, IIdentifiable
{
    private readonly List<T> items;
    public Repository(){
        items = new List<T>();
    }

    /// <summary>
    /// Возвращает текущее число элементов в репозитории.
    /// </summary>
    public int Count
    {
        get{return items.Count;}
    }

    /// <summary>
    /// Возвращает элемент по его идентификатору.
    /// </summary>
    public T? this[Guid id]
    {
        get{
            foreach (T item in items){
                if (item.Id == id)
                    return item;
            }
            return null;
        }
    }

    /// <summary>
    /// Возвращает сущность по идентификатору.
    /// </summary>
    public T? GetById(Guid id){
        return this[id];
    }

    /// <summary>
    /// Возвращает все сущности в репозитории.
    /// </summary>
    public IEnumerable<T> GetAll(){
        foreach (T item in items)
            yield return item;
    }

    /// <summary>
    /// Возвращает элемент по индексу.
    /// </summary>
    public T GetAt(int index)
    {
        if (index < 0 || index >= items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return items[index];
    }

    /// <summary>
    /// Добавляет новую сущность в репозиторий.
    /// </summary>
    public void Add(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        if (this[item.Id] is not null)
            throw new InvalidOperationException("Объект с таким идентификатором уже существует.");

        items.Add(item);
    }

    /// <summary>
    /// Удаляет сущность по идентификатору.
    /// </summary>
    public bool Remove(Guid id)
    {
        T? item = this[id];
        if (item is null)
            return false;

        items.Remove(item);
        return true;
    }

    /// <summary>
    /// Находит все сущности, которые удовлетворяют заданному предикату.
    /// </summary>
    public IReadOnlyCollection<T> FindAll(Func<T, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        List<T> result = new List<T>();
        foreach (T item in items){
            if (predicate(item))
                result.Add(item);
        }

        return result.AsReadOnly();
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator(){
        return items.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator(){
        return GetEnumerator();
    }

}
