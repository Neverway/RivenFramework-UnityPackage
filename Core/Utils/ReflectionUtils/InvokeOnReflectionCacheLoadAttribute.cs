using System;

namespace RivenFramework.Utils.Reflection
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class InvokeOnReflectionCacheLoadAttribute : Attribute
    {
        public int priority;
        public InvokeOnReflectionCacheLoadAttribute(int priority = 0)
        {
            this.priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class InvokeOnReflectionCacheLoadRuntimeAttribute 
        : InvokeOnReflectionCacheLoadAttribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class InvokeOnReflectionCacheLoadEditorAttribute
        : InvokeOnReflectionCacheLoadAttribute { }
}
