using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RivenFramework
{
    public static class VisualElementExtensionMethods
    {
        public static void On<TEvent>(this VisualElement ve, EventCallback<TEvent> onEvent)
            where TEvent : EventBase<TEvent>, new() => ve.RegisterCallback(onEvent);
        public static void OnMouseEnter(this VisualElement ve, EventCallback<MouseEnterEvent> onMouseEnter) =>
            ve.RegisterCallback(onMouseEnter);
        public static void OnMouseLeave(this VisualElement ve, EventCallback<MouseLeaveEvent> onMouseEnter) =>
            ve.RegisterCallback(onMouseEnter);
        public static void OnMouseDown(this VisualElement ve, EventCallback<MouseDownEvent> onMouseEnter) =>
            ve.RegisterCallback(onMouseEnter);
        public static void OnMouseUp(this VisualElement ve, EventCallback<MouseUpEvent> onMouseEnter) =>
            ve.RegisterCallback(onMouseEnter);


        public static void SetBorderWidth(this IStyle style, float width)
        {
            style.borderTopWidth = width;
            style.borderBottomWidth = width;
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
        }
        public static void SetBorderRadius(this IStyle style, float radius)
        {
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
        }
        public static void SetBorderColor(this IStyle style, StyleColor color)
        {
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
        }
        public static void SetPadding(this IStyle style, float padding)
        {
            style.paddingTop = padding;
            style.paddingBottom = padding;
            style.paddingLeft = padding;
            style.paddingRight = padding;
        }
        public static void SetMargin(this IStyle style, float margin)
        {
            style.marginTop = margin;
            style.marginBottom = margin;
            style.marginLeft = margin;
            style.marginRight = margin;
        }




        public static void Hide(this VisualElement ve) 
            => ve.style.display = DisplayStyle.None;

        public static void Show(this VisualElement ve)
            => ve.style.display = DisplayStyle.Flex;

        public static void SetVisible(this VisualElement ve, bool visibility)
            => ve.style.display = visibility ? DisplayStyle.Flex : DisplayStyle.None;

    }
}
