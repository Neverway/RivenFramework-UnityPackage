using UnityEditor;
using UnityEngine;

    [InitializeOnLoad]
    public static class SimpleFieldsOnSceneGUIRegister
    {
        static SimpleFieldsOnSceneGUIRegister()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                GameObject selected = Selection.activeGameObject;
                if (selected != null)
                {
                    SimpleFields target = selected.GetComponent<SimpleFields>();
                    if (target != null)
                    {
                        Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                        SimpleFieldsWindow.DisplayPopup(target);
                        e.Use();
                    }
                }
            }
        }
    }

