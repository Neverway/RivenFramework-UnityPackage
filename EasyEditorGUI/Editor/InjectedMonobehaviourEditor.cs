using RivenFramework.Utils.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static EasyPropertyDrawer;
using Object = UnityEngine.Object;

namespace EasyEditorGUI
{
    //[CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class InjectedMonobehaviourEditor : Editor
    {
        public Type monoType { get; private set; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            monoType = target.GetType();

            foreach (DrawerItem item in GetItemsToDraw())
                item.Draw();

            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<DrawerItem> GetItemsToDraw()
        {
            List<DrawerItem> itemsToDraw = new List<DrawerItem>();
            HashSet<MemberInfo> serializedFields = new HashSet<MemberInfo>();
            int onwardInspectorOrder = 0;

            // Collect visible root-level properties
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            if (iterator.NextVisible(true))
            {
                while (iterator.NextVisible(enterChildren))
                {
                    FieldInfo field = iterator.GetFieldInfo();
                    serializedFields.Add(field);

                    int order = onwardInspectorOrder;
                    if (field.HasAttribute(out InspectorOrderAttribute newOrder, false))
                        order = newOrder.inspectorOrder;
                    if (field.HasAttribute(out InspectorOrderOnwardAttribute newOnwardOrder, false))
                    {
                        onwardInspectorOrder = newOnwardOrder.inspectorOrder;
                        if (newOrder == null)
                            order = onwardInspectorOrder;
                    }

                    itemsToDraw.Add(PropertyToDrawerItem(iterator.Copy(), field, order));

                    enterChildren = false;
                }
            }

            // Collect drawers off of attributes from each member in the MonoBehaviour
            foreach (MemberInfo member in monoType.GetCachedMemberInfos())
            {
                if (serializedFields.Contains(member)) continue;
                if (member.HasAttribute<HideInInspector>()) continue;

                //Get the inspector order of the member
                int order = 0;
                if (member.HasAttribute(out InspectorOrderAttribute newOrder, false))
                    order = newOrder.inspectorOrder;

                //Get the visual contents of the member
                VerticalGroup contents = new VerticalGroup();
                foreach (AttributeUsage usage in member.GetCachedAttributeUsages())
                    if (usage.Attribute is IMemberAttributeDrawer attribute)
                        contents.Add(attribute.GetDrawer(member, target));

                itemsToDraw.Add(new DrawerItem(contents, order));
            }

            return itemsToDraw.OrderBy(item => item.inspectorOrder);
        }

        private DrawerItem PropertyToDrawerItem(SerializedProperty prop, FieldInfo field, int order)
        {
            VerticalGroup contents = new VerticalGroup();
            contents.Add(new Property(prop).IncludeChildren());
            var propDrawers = field.GetAttributes<IPropertyAttributeDrawer>(true);

            foreach (var propDrawer in propDrawers)
                contents = propDrawer.UpdateDrawer(contents, prop);

            return new DrawerItem(contents, order);
        }

        private struct DrawerItem
        {
            public int inspectorOrder { get; private set; }
            private DrawerObject drawer;

            public DrawerItem(DrawerObject drawer, int inspectorOrder)
            {
                this.drawer = drawer;
                this.inspectorOrder = inspectorOrder;
            }

            public void Draw() => drawer.Draw();
        }
    }
}


[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute, IMemberAttributeDrawer
{
    public string customName;
    private MethodInfo cachedMethod;
    private Object cachedTarget;

    public ButtonAttribute(string customName = null)
    {
        this.customName = customName;
    }

    public DrawerObject GetDrawer(MemberInfo member, Object target)
    {
        if (member is not MethodInfo method)
            throw new ArgumentException();

        cachedTarget = method.IsStatic ? target : null;
        cachedMethod = method;
        string buttonName = (customName == null ? method.HumanName() : customName);
        return new Button(buttonName, OnPressedButton);
    }
    public void OnPressedButton() => cachedMethod.Invoke(cachedTarget, null);
}






[AttributeUsage(AttributeTargets.Field)]
public class BoxedAttribute : Attribute, IPropertyAttributeDrawer
{
    public VerticalGroup UpdateDrawer(VerticalGroup contents, SerializedProperty property)
    {
        return new VerticalGroup(new Boxed(contents));
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class AddLabelAttribute : Attribute, IPropertyAttributeDrawer
{
    public string customName;

    public AddLabelAttribute(string customName)
    {
        this.customName = customName;
    }
    public VerticalGroup UpdateDrawer(VerticalGroup contents, SerializedProperty property)
    {
        contents.AddAbove(new Label(customName));
        return contents;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class MustNotBeNullAttribute : Attribute, IPropertyAttributeDrawer
{
    public VerticalGroup UpdateDrawer(VerticalGroup contents, SerializedProperty property)
    {
        if (property.boxedValue == null)
            contents.AddAbove(new Label("<color=red>MUST NOT BE NULL</color>").UseRichText());

        return contents;
    }
}











public interface IPropertyAttributeDrawer
{
    public VerticalGroup UpdateDrawer(VerticalGroup contents, SerializedProperty property);
}
public interface IMemberAttributeDrawer
{
    public DrawerObject GetDrawer(MemberInfo member, Object target);
}
public interface IAttributeDrawer { }

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class InspectorOrderAttribute : Attribute
{
    public int inspectorOrder;

    public InspectorOrderAttribute(int inspectorOrder)
    {
        this.inspectorOrder = inspectorOrder;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InspectorOrderOnwardAttribute : PropertyAttribute
{
    public int inspectorOrder;

    public InspectorOrderOnwardAttribute(int inspectorOrder)
    {
        this.inspectorOrder = inspectorOrder;
    }
}




[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class OrderAttribute : Attribute
{
    public int order;
    public OrderAttribute(int order)
    {
        this.order = order;
    }
}