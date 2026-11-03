
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods_Filter
{
    public static IEnumerable<T> GetFiltered<T>(this IEnumerable<T> collection, Filter<T> filter)
    {
        foreach (var item in collection)
            if (filter.PassesFilter(item))
                yield return item;
    }

    public static bool PassesAllFilters<T>(this IEnumerable<Filter<T>> allFilters, T item)
    {
        foreach (var filter in allFilters)
            if (!filter.PassesFilter(item))
                return false;
        return true;
    }
    public static bool PassesAnyFilter<T>(this IEnumerable<Filter<T>> allFilters, T item)
    {
        foreach (var filter in allFilters)
            if (filter.PassesFilter(item))
                return true;
        return false;
    }
}

[Serializable]
public abstract class Filter<T>
{
    public abstract bool PassesFilter(T _objectToCheck);
}

public interface MultiFilter<TFilter, T> where TFilter : Filter<T>
{
    [field: SerializeField] public FilterCompareType filterCompareType { get; set; }
    [field: SerializeReference, Polymorphic] public List<TFilter> filters { get; set; }

    public enum FilterCompareType
    {
        MustPassAll,
        PassesAny
    }
    
    public bool PassesMultiFilter(T _object)
    {
        switch (filterCompareType)
        {
            case FilterCompareType.MustPassAll:
                return filters.PassesAllFilters(_object);
            case FilterCompareType.PassesAny:
                return filters.PassesAnyFilter(_object);
        }

        return true;
    }
}