using MarkupAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace RivenFramework
{
    [CustomEditor(typeof(ScriptableObject), true), CanEditMultipleObjects]
    public class MarkedUpScriptableObjectEditor : MarkedUpEditor
    {

    }
}