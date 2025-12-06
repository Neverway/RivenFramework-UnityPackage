//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using RivenFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Todo("Finish implementing generic widget references", Owner = "Errynei")]
public class GI_WidgetManager : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public List<GameObject> widgets;

    //=-----------------=
    // Private Variables
    //=-----------------=
    private const string CANVAS_GAMEOBJECT_TAG = "UserInterface";
    private GameObject _canvas;
    private GameObject Canvas
    {
        get
        {
            if (_canvas == null)
                _canvas = GameObject.FindWithTag(CANVAS_GAMEOBJECT_TAG);
            return _canvas;
        }
        
    }

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=


    //=-----------------=
    // Internal Functions
    //=-----------------=
    /// <summary>
    /// Find the widget prefab, on the widget manager, with the specified name
    /// </summary>
    private GameObject GetWidgetPrefab(string _widgetName)
    {
        foreach (var widget in widgets)
            if (widget.name == _widgetName) 
                return widget;

        throw new Exception($"No widget named \"{_widgetName}\" exists. " +
            $"(check if widget is added to {nameof(GI_WidgetManager)} on {name})");
    }
    /// <summary>
    /// Find the widget prefab, on the widget manager, using the widget's unique WB script
    /// </summary>
    private GameObject GetWidgetPrefab<T>() where T : WidgetBlueprint
    {
        foreach (var widget in widgets)
            if (widget.GetComponent<T>() != null)
                return widget;

        throw new Exception($"No widget of type \"{nameof(T)}\" exists. " +
            $"(check if widget is added to {nameof(GI_WidgetManager)} on {name})");
    }

    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Add the widget from the widget manager, with the specified name, to the user interface
    /// </summary>
    /// <param name="_allowDuplicates">If enabled, multiple of the same widget can be added to the UI</param>
    /// <returns>True if adding the widget was successful
    /// <p>False if the widget couldn't be found on the widget manager</p>
    /// <p>(The result will also be False if the widget was already present on the UI and allowDuplicates is False)</p> </returns>
    public bool AddWidget(string _widgetName, bool _allowDuplicates = false) => 
        AddWidget(GetWidgetPrefab(_widgetName), _allowDuplicates);
    
    /// <summary>
    /// Add the widget from the widget manager, with the specified WB script, to the user interface
    /// </summary>
    /// <param name="_allowDuplicates">If enabled, multiple of the same widget can be added to the UI</param>
    /// <returns>True if adding the widget was successful
    /// <p>False if the widget couldn't be found on the widget manager</p>
    /// <p>(The result will also be False if the widget was already present on the UI and allowDuplicates is False)</p> </returns>
    public bool AddWidget<T>(bool _allowDuplicates = false) where T : WidgetBlueprint =>
        AddWidget(GetWidgetPrefab<T>(), _allowDuplicates);
    /// <summary>
    /// Add the widget from the widget manager, with the same name as the specified object, to the user interface
    /// </summary>
    /// <param name="_allowDuplicates">If enabled, multiple of the same widget can be added to the UI</param>
    /// <returns>True if adding the widget was successful
    /// <p>False if the widget couldn't be found on the widget manager</p>
    /// <p>(The result will also be False if the widget was already present on the UI and allowDuplicates is False)</p> </returns>
    public bool AddWidget(GameObject _widgetObject, bool _allowDuplicates = false)
    {
        //Do not add widget if no canvas exists
        if (Canvas == null) return false;

        //Do not allow adding a new widget if it already exists (unless duplicates are allowed)
        if (_allowDuplicates is false)
            if (GetExistingWidget(_widgetObject.name) != null)
                return false;

        var newWidget = Instantiate(_widgetObject, Canvas.transform, false);
        newWidget.transform.localScale = Vector3.one;
        newWidget.name = _widgetObject.name;
        return true;
    }

    /// <summary>Gets the widget of the specified type (or creates a new one if one does not exist)
    /// <br/>Returns false if widget could not be created or retrieved (like if the Canvas was null)</summary>
    [Todo_AddComments("Ported from AuHo, Needs tidying")]
    public bool AddOrGetExistingWidget<T>(out T addedWidget) where T : MonoBehaviour
    {
        addedWidget = null; //Initialize with default value
        //Do not add widget if no canvas exists
        if (Canvas == null) return false;

        //Try finding an existing widget of that type
        foreach (Transform child in Canvas.transform)
        {
            addedWidget = child.GetComponent<T>();
            if (addedWidget != null) return true;
        }
        
        //Try to get the widget prefab
        T widgetPrefab = null;
        foreach (var widget in widgets) 
            if (widget.TryGetComponent(out widgetPrefab)) 
                break;
        if (widgetPrefab == null)
        {
            Debug.LogError($"No Widget with component of type {typeof(T)} was found. Maybe you forgot to" +
                           $"add it to {nameof(GI_WidgetManager)}?");
            return false;
        }

        GameObject widgetObj = Instantiate(widgetPrefab.gameObject, Canvas.transform, false);
        T newWidget = widgetObj.GetComponent<T>();
        newWidget.transform.localScale = Vector3.one;
        newWidget.name = widgetPrefab.name;
        addedWidget = newWidget;
        return true;
    }
    
    
    /// <summary>
    /// Add the widget from the UI, with the specified name, or remove it if it's already present
    /// </summary>
    /// <returns>Returns true if the widget was added and false if it was removed</returns>
    public bool ToggleWidget(string _widgetName)
    {
        // If the widget already exists, destroy it
        GameObject existingWidget = GetExistingWidget(_widgetName);
        if (existingWidget != null)
        {
            Destroy(existingWidget);
            return false;
        }
        // If it does not exist, create it
        AddWidget(_widgetName);
        return true;
    }
    /// <summary>
    /// Add the widget from the UI, with the specified WB script, or remove it if it's already present
    /// </summary>
    /// <returns>Returns true if the widget was added and false if it was removed</returns>
    public bool ToggleWidget<T>() where T : WidgetBlueprint =>
        ToggleWidget(GetWidgetPrefab<T>().name);

    
    /// <summary>
    /// Get the specified widget object if it's present on the user interface
    /// </summary>
    /// <returns>Returns the widget object from the UI</returns>
    public GameObject GetExistingWidget(string _widgetName)
    {
        if (Canvas == null) return null;

        foreach (Transform child in Canvas.transform)
            if (child.name == _widgetName) return child.gameObject;

        return null;
    }
    /// <summary>
    /// Get the specified widget object if it's present on the user interface
    /// </summary>
    /// <returns>Returns the widget object from the UI</returns>
    public T GetExistingWidget<T>() where T : WidgetBlueprint
    {
        if (Canvas == null) return null;

        foreach (Transform child in Canvas.transform)
        {
            T widget = child.GetComponent<T>();
            if (widget != null) return widget;
        }
        return null;
    }
}
