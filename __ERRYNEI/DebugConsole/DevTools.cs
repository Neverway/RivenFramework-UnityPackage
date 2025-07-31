using System;
using System.Reflection;
using UnityEngine;

public class DevTools
{
    private static MenuOption[] allMenuOptions;

    [RuntimeInitializeOnLoadMethod]
    public static void DomainReloadMenuOptions() => allMenuOptions = null;
    public static MenuOption[] GetAllMenuOptions()
    {
        //DO STUFF HERE
         
        return allMenuOptions;
    }

    private static MenuOption GetMenuOptionFromStaticField(FieldInfo field)
    {
        return null;
    }
    private static MenuOption GetMenuOptionFromInstanceField(FieldInfo field)
    {

        return null;
    }
    private static MenuOption GetMenuOptionFromStaticField(MethodInfo field)
    {

        return null;
    }
    private static MenuOption GetMenuOptionFromInstanceField(MethodInfo field)
    {

        return null;
    }

    public abstract class MenuOption
    {

    }
    public class MenuButton : MenuOption
    {

    }
    public class MenuToggle : MenuOption
    {

    }
}

public abstract class DevMenuAttribute : Attribute
{
    public string name;
    public string tabGroup;
    public string category;
    public DevMenuAttribute(string name = "", string tabGroup = "", string category = "")
    {

    }

    public abstract DevTools.MenuOption GetMenuOption(MemberInfo memberInfo);
}
[AttributeUsage(AttributeTargets.Method)]
public class DevMenuButtonAttribute : DevMenuAttribute
{

    public override DevTools.MenuOption GetMenuOption(MemberInfo memberInfo)
    {
        if (memberInfo is MethodInfo methodInfo)
        {

        }
        return new DevTools.MenuButton();
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class DevMenuToggleAttribute : DevMenuAttribute
{
    public override DevTools.MenuOption GetMenuOption(MemberInfo memberInfo)
    {
        return new DevTools.MenuToggle();
    }
}