using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RivenFramework.Utils
{
    public static class ReflectionUtils
    {
        private static Type[] allTypes;
        private static FieldInfo[] staticFields;
        private static FieldInfo[] instanceFields;
        private static MethodInfo[] staticMethods;
        private static MethodInfo[] instanceMethods;
        private static EventInfo[] staticEvents;
        private static EventInfo[] instanceEvents;

        private static Type AttributesOnLoadGenericType => typeof(IGetAttributesWithMemberInfosOnLoad<,>);
        private static string AttributesOnLoadMethod = 
            nameof(IGetAttributesWithMemberInfosOnLoad<Attribute, MemberInfo>.OnGetAttributesWithInfoOnLoad);

        //[RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        //[InitializeOnLoadMethod]
#endif
        public static void InitializeOnLoad()
        {
            GetMemberInfos();

            foreach(Type t in allTypes)
            {
                if (!t.IsGenericType)
                    continue;

                if (AttributesOnLoadGenericType.IsAssignableFrom(t.GetGenericTypeDefinition()))
                {
                    Attribute attribute = t.GetCustomAttribute(t.GetGenericArguments()[0]);
                    if (attribute == null)
                        continue;
                }
            }
            Debug.Log(AttributesOnLoadGenericType + "" + AttributesOnLoadMethod);
        }
        private static void GetMemberInfos()
        {
            BindingFlags staticMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            BindingFlags instanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()).ToArray();

            staticFields = allTypes
                    .SelectMany(type => type.GetFields(staticMembers)).ToArray();

            instanceFields = allTypes
                    .SelectMany(type => type.GetFields(instanceMembers)).ToArray();

            staticMethods = allTypes
                    .SelectMany(type => type.GetMethods(staticMembers)).ToArray();

            instanceMethods = allTypes
                    .SelectMany(type => type.GetMethods(staticMembers)).ToArray();

            staticEvents = allTypes
                    .SelectMany(type => type.GetEvents(staticMembers)).ToArray();

            instanceEvents = allTypes
                    .SelectMany(type => type.GetEvents(staticMembers)).ToArray();
        }
    }
}
