using RivenFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// todo: NOT DONE, IMPLEMENT ARRAYS
/// </summary>
[Todo("Check if this actually works, and either use it or toss it", Owner = "Errynei")]
public class FetchComponentAttribute : PropertyAttribute 
{
    private bool searchChildren;
    private bool searchParents;
    private Type type;

    public FetchComponentAttribute(bool children = false, bool parents = false, Type type = null)
    {
        this.searchChildren = children;
        this.searchParents = parents;
        this.type = type;
    }
    public Component Fetch(Component homeComponent, Type defaultType)
    {
        if (homeComponent == null) return null;
        if (type == null) type = defaultType;
        if (type == null) return null;

        return searchChildren ? 
            (searchParents ?
                GetFromParentsAndChildren(homeComponent) : 
                homeComponent.GetComponentInChildren(type)) :
            (searchParents ? 
                homeComponent.GetComponentInParent(type) : 
                homeComponent.GetComponent(type));
    }
    public Component[] FetchArray(Component homeComponent, Type defaultType)
    {
        if (homeComponent == null) return null;
        if (type == null) type = defaultType;
        if (type == null) return null;

        List<Component> list = new List<Component>();

        list.AddRange(homeComponent.GetComponents(type));
        if (searchParents)
            list.AddRange(homeComponent.GetComponentsInParent(type).Where(c => !list.Contains(c)));
        if (searchChildren)
            list.AddRange(homeComponent.GetComponentsInChildren(type).Where(c => !list.Contains(c)));

        return list.ToArray();
    }

    private Component GetFromParentsAndChildren(Component homeComponent)
    {
        Component fetchedComponent = homeComponent.GetComponentInParent(type);
        if (fetchedComponent == null)
            fetchedComponent = homeComponent.GetComponentInChildren(type);

        return fetchedComponent;
    }
}