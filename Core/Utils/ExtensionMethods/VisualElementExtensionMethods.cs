using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    public static class VisualElementExtensionMethods
    {
        public static void Hide(this VisualElement ve) 
            => ve.style.display = DisplayStyle.None;

        public static void Show(this VisualElement ve)
            => ve.style.display = DisplayStyle.Flex;

        public static void SetVisible(this VisualElement ve, bool visibility)
            => ve.style.display = visibility ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
