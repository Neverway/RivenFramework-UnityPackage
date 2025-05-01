//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GI_WidgetManager : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public List<GameObject> widgets;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Adds the specified widget to the user interface if it's in the widget list
    /// </summary>
    /// <param name="_widgetName"></param>
    /// <returns>Returns true if we added the widget and false if it failed to be added (or if it was already present and _allowDuplicates is set to false)</returns>
    public bool AddWidget(string _widgetName, bool _allowDuplicates = false)
    {
        var canvas = GameObject.FindWithTag("UserInterface");
        if (_allowDuplicates is false && GetExistingWidget(_widgetName)) return false;
        if (canvas)
        {
            foreach (var widget in widgets)
            {
                if (widget.name == _widgetName)
                {
                    var newWidget = Instantiate(widget, canvas.transform, false);
                    newWidget.transform.localScale = new Vector3(1, 1, 1);
                    newWidget.name = newWidget.name.Replace("(Clone)", "").Trim();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Adds the specified widget if it's no present on the interface, or removes it if it already is
    /// </summary>
    /// <param name="_widgetName"></param>
    /// <returns>Returns true if we added the widget and false if we destroyed it</returns>
    public bool ToggleWidget(string _widgetName)
    {
        var canvas = GameObject.FindWithTag("UserInterface");
        // If the widget already exists, destroy it
        if (GetExistingWidget(_widgetName))
        {
            Destroy(GetExistingWidget(_widgetName));
            return false;
        }
        // If it does not exist, create it
        AddWidget(_widgetName);
        return true;
    }
    
    /// <summary>
    /// Returns the specified widget object if the widget is present on the interface
    /// </summary>
    /// <param name="_widgetName"></param>
    /// <returns>Returns the widget if it's present on the user interface</returns>
    public GameObject GetExistingWidget(string _widgetName)
    {
        var canvas = GameObject.FindWithTag("UserInterface");
        if (canvas)
        {
            for (var i = 0; i < canvas.transform.childCount; i++)
            {
                var widget = canvas.transform.GetChild(i).gameObject;
                if (widget.name == _widgetName) return widget;
            }
        }
        return null;
    }
}
