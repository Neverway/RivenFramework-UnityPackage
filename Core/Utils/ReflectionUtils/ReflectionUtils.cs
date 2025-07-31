using UnityEngine;
using System.Reflection;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RivenFramework.Utils.Reflection
{
    public static class ReflectionUtils
    {
        private static string assetsPath;
        private static readonly string libraryScriptAssembliesLocation = "/Library/ScriptAssemblies/";

        private static void Initialize()
        {
            //Calculate helper fields once to be used later in other functions
            assetsPath = Application.dataPath.Replace('\\', '/');

            //Reset the Reflection Cache to make its up to date with the current compiled scripts
            ReflectionCache.Reset();
        }

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void EditorLoad() => Load(false);
        #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RuntimeLoad() => Load(true);

        public static void Load(bool isRuntime)
        {
            Initialize();

            AttributeUsage[] invokeOnLoadUsages =
                //Get all usages of the [InvokeOnReflectionCacheLoad] attribute
                ReflectionCache.GetAttributeUsages<InvokeOnReflectionCacheLoadAttribute>();

            if (invokeOnLoadUsages == null)
            {
                Debug.Log("Was null???");
                return;
            }

                invokeOnLoadUsages = invokeOnLoadUsages
                //Ignore runtime/editor versions of the attribute depending on if whether this is currently runtime or editor
                //.Where(usage => !(isRuntime && usage.Attribute is InvokeOnReflectionCacheLoadEditorAttribute))
                //.Where(usage => !(!isRuntime && usage.Attribute is InvokeOnReflectionCacheLoadRuntimeAttribute))
                //Sort array by the attributes defined priority, where a lower priority is sooner in the list
                .OrderBy(usage => usage.As<InvokeOnReflectionCacheLoadAttribute>().priority)
                .ToArray();

            foreach (AttributeUsage usage in invokeOnLoadUsages)
            {
                //Ignore runtime/editor versions of the attribute depending on if whether this is currently runtime or editor
                if(isRuntime && usage.Attribute is InvokeOnReflectionCacheLoadEditorAttribute) continue;
                if(!isRuntime && usage.Attribute is InvokeOnReflectionCacheLoadRuntimeAttribute) continue;


                if (usage.Member is MethodInfo method && method.IsStatic && method.GetParameters().Length == 0)
                {
                    method.Invoke(null, null);
                }
            }
        }

        private static readonly Dictionary<string, Type> aliasToType = new()
        {
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "char", typeof(char) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "object", typeof(object) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "string", typeof(string) },
            { "void", typeof(void) }
        };
        private static readonly Dictionary<Type, string> typeToAlias = new()
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(object), "object" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(void), "void" }
        };

        public static bool IsLikelyValidVariableName(this Type type, string name)
        {
            //TODO: NOT DONEEE



            string ungenericTypeName = type.NameWithoutGenericOrArray();
            // 1. Check against alias dictionary
            if (aliasToType.TryGetValue(name, out var aliasedType))
                return type == aliasedType;

            // 2. Check simple name (e.g., "Int32")
            if (type.Name == name)
                return true;

            // 3. Check full name (e.g., "System.Int32")
            if (name.EndsWith($".{type.Name}"))
                return true;

            return false;
        }
         /*
        public class Tree<T> : IEnumerator<T>
        {
            private T startPoint;
            private Func<T, IEnumerator<T>> branchFunc;
            private Stack<IEnumerator<T>> tree;
            private int index;
            public Tree(Func<T, IEnumerator<T>> branchFunc, T startPoint)
            {
                tree = new Stack<Queue<IEnumerator<T>>>();
            }

            public T Current => tree.Peek().Current;
            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (tree.TryPeek(out var branch))
                {
                    if (branch.MoveNext())
                        branch.Pop();
                }
            }
            public void Reset()
            {
                tree.Clear();
                tree.Push(branchFunc.Invoke(startPoint));
            }
        }
        public class SingleItem<T> : IEnumerator<T>
        {
            private T item;
            private bool returned = false;
            public SingleItem(T item) => this.item = item;
            public T Current => item;
            object IEnumerator.Current => Current;
            public void Dispose() { }
            public bool MoveNext() => returned ? false : returned = true;
            public void Reset() => returned = false;
        }
        

        public static void TreeStep<T>(this T start, 
            Func<T, T[]> treeFunc,
            Action<T> onOpen = null, 
            Action<T> onClose = null,
            Action<T> onEndPoint = null)
        {

            Stack<T> closingItems = new Stack<T>();
            Stack<Queue<T>> stack = new Stack<Queue<T>>();
            Queue<T> queue = new Queue<T>();
            T item = start;
            queue.Enqueue(item);
            stack.Push(queue);

            while (stack.Count > 0)
            {
                queue = stack.Pop();

                if (queue.Count == 0)
                {
                    item = closingItems.Pop();
                    onClose?.Invoke(item);
                    continue;
                }

                item = queue.Dequeue();

                T[] subItems = treeFunc.Invoke(item);

                if (subItems.Length > 1)
                {
                    onOpen?.Invoke(item);
                    closingItems.Push(item);

                    stack.Push(queue);
                    stack.Push(new Queue<T>(subItems));
                    continue;
                }
                else if (subItems.Length == 1)
                {
                    onEndPoint?.Invoke(subItems[0]);
                }
                else
                {
                    onEndPoint?.Invoke(item);
                }
            }
        }
        // */
        public static string NameWithGenericAndArray(this Type type)
        {
            StringBuilder sb = new StringBuilder();
            Stack<Queue<Type>> typeStackQueue = new Stack<Queue<Type>>();
            Stack<Queue<int>> arrayCount = new Stack<Queue<int>>();

            Queue<Type> currentQueue = new Queue<Type>();
            currentQueue.Enqueue(type);

            typeStackQueue.Push(currentQueue);
            while(typeStackQueue.Count > 0)
            {
                currentQueue = typeStackQueue.Pop();

                if (currentQueue.Count == 0)
                {
                    if (typeStackQueue.Count == 0)
                        break;

                    Queue<int> arrayQueue = arrayCount.Pop();
                    foreach (int brackets in arrayQueue)
                    {
                        sb.Append('[');
                        for (int commas = brackets - 1; commas > 0; commas--)
                            sb.Append(',');
                        sb.Append(']');
                    }

                    sb.Append(">");
                    if (typeStackQueue.TryPeek(out Queue<Type> nextQueue))
                        if (nextQueue.Count > 0)
                            sb.Append(", ");
                    continue;
                }

                Type t = currentQueue.Dequeue();
                Queue<int> arrayRanks = new Queue<int>();
                while (t.IsArray)
                {
                    arrayRanks.Enqueue(t.GetArrayRank());
                    t = t.GetElementType();
                }
                arrayCount.Push(arrayRanks);

                if (!typeToAlias.TryGetValue(t, out var aliasedName))
                    aliasedName = t.NameWithoutGenericOrArray();
                sb.Append(aliasedName);

                if (t.IsGenericType)
                {
                    sb.Append("<");
                    typeStackQueue.Push(currentQueue);
                    typeStackQueue.Push(new Queue<Type>(t.GetGenericArguments()));
                    continue;
                }
                else
                {
                    Queue<int> arrayQueue = arrayCount.Pop();
                    foreach (int brackets in arrayQueue)
                    {
                        sb.Append('[');
                        for (int commas = brackets - 1; commas > 0; commas--)
                            sb.Append(',');
                        sb.Append(']');
                    }
                }
                if (currentQueue.Count > 0)
                    sb.Append(", ");

                typeStackQueue.Push(currentQueue);
            }
            return sb.ToString();
        }
        public static string NameWithoutGenericOrArray(this Type type)
        {
            string name = type.Name;
            int index = name.IndexOf('`');
            if (index == -1)
                return name;
            return name.Substring(0, index);
        }

        public static IEnumerable<Type> GetAllTypesAssignableTo(this Type type)
        {
            //beepbopboopbopbeepbop >:3   (credit to Jen, this helps the code work better)
            Type baseTypes = type;
            while (baseTypes != null)
            { 
                yield return baseTypes;
                baseTypes = baseTypes.BaseType;
            }
            foreach (Type interfaceType in type.GetInterfaces())
                yield return interfaceType;
        }

        public static bool IsInvokeableParameterlessStatic(this MethodInfo method) =>
            method.HasParametersNone() && !method.IsAbstract;
        public static bool IsInvokeableParameterless(this MethodInfo method) =>
            method.HasParametersNone() && !method.IsAbstract;

        public static bool HasParameters<T1, T2, T3, T4>(this MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 4) return false;
            if (!parameters[0].ParameterType.IsAssignableFrom(typeof(T1))) return false;
            if (!parameters[1].ParameterType.IsAssignableFrom(typeof(T2))) return false;
            if (!parameters[2].ParameterType.IsAssignableFrom(typeof(T3))) return false;
            if (!parameters[3].ParameterType.IsAssignableFrom(typeof(T4))) return false;

            return true;
        }
        public static bool HasParameters<T1, T2, T3>(this MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 3) return false;
            if (!parameters[0].ParameterType.IsAssignableFrom(typeof(T1))) return false;
            if (!parameters[1].ParameterType.IsAssignableFrom(typeof(T2))) return false;
            if (!parameters[2].ParameterType.IsAssignableFrom(typeof(T3))) return false;

            return true;
        }
        public static bool HasParameters<T1, T2>(this MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 2) return false;
            if (!parameters[0].ParameterType.IsAssignableFrom(typeof(T1))) return false;
            if (!parameters[1].ParameterType.IsAssignableFrom(typeof(T2))) return false;

            return true;
        }
        public static bool HasParameters<T1>(this MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1) return false;
            if (!parameters[0].ParameterType.IsAssignableFrom(typeof(T1))) return false;

            return true;
        }
        public static bool HasParametersNone(this MethodInfo method) =>
            method.GetParameters().Length == 0;

        #region Assembly Dependency Check Methods

        public static void DEBUG_PrintFolderLocation(this Assembly assembly)
        {
            if (assembly.IsDynamic)
            {
                Debug.Log(assembly.FullName + " is a dynamic assembly, no folder location available");
                return;
            }
            Debug.Log(assembly.Location.Replace('\\', '/'));
        }
        public static void DEBUG_PrintFolderLocation(this Assembly assembly, bool passesCheck)
        {
            if (assembly.IsDynamic)
            {
                Debug.Log(assembly.GetName().Name + " is a dynamic assembly, no folder location available");
                return;
            }
            StringBuilder result = new StringBuilder();
            result.Append(passesCheck ? "<color=#88ff88>" : "<color=#ff8888>");
            result.Append(assembly.Location.Replace('\\', '/'));
            result.Append("</color>");
            Debug.Log(result.ToString());
        }

        /// <summary>
        /// Tells you if an <see cref="Assembly"/> is a part of Unity's assemblies, specifically, ones starting with
        /// "Unity", "UnityEngine", or "UnityEditor"
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to check if is a Unity assembly
        /// </param>
        /// <returns>true: if the name of the assembly starts with "Unity", "UnityEngine", or "UnityEditor"
        /// </returns>
        public static bool IsUnityAssembly(this Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name;
            int indexOf = assemblyName.IndexOf('.');
            if (indexOf != -1)
                assemblyName = assemblyName.Substring(0, indexOf);
            return assemblyName == "Unity" || assemblyName == "UnityEngine" || assemblyName == "UnityEditor";

        }
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks to see if its located in ../Library/ScriptAssemblies. This
        /// contains Unity's assemblies, Package assemblies, default assemblies, and assemblies from asmdef files in
        /// your assets folder. (Does not include any dynamic assemblies since they don't have a location)
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking if is a project script assembly
        /// </param>
        /// <returns>true: if this assembly's location is inside ../Library/ScriptAssemblies/ (and if its not dynamic)
        /// </returns>
        public static bool IsProjectScriptAssembly(this Assembly assembly)
        {
            //Do not use any dynamic assemblies. This avoids an error when
            //checking for its location and is not an assembly you'd usually want to use anyways
            if (assembly.IsDynamic)
                return false;

            //Use the assembly if it is inside the Assets folder
            string assemblyLocation = assembly.Location.Replace('\\', '/');
            return assemblyLocation.Contains(libraryScriptAssembliesLocation);
        }
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks if the assembly containing this <see cref="ReflectionUtils"/> 
        /// class is one of its referenced assemblies
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking for 
        /// a dependency to the assembly that contains <see cref="ReflectionUtils"/></param>
        /// <returns>true: if this assembly is dependent on the assembly containing <see cref="ReflectionUtils"/>
        /// </returns>
        internal static bool IsDependentOnReflectionUtils(this Assembly assembly) =>
            assembly.IsDependentOnOtherAssembly(Assembly.GetExecutingAssembly().FullName);
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks its references for a match with the assembly that contains
        /// the code that invoked this method.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking for a dependency</param>
        /// <returns>true: if this assembly is dependent on the assembly from which you called this function from</returns>
        public static bool IsDependentOnThisCurrentAssembly(this Assembly assembly) =>
            assembly.IsDependentOnOtherAssembly(Assembly.GetCallingAssembly().FullName);
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks its references for a match with some other provided assembly
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking for a dependency</param>
        /// <param name="otherAssembly">The <see cref="Assembly"/> you are looking for a dependency for</param>
        /// <returns>true: if this assembly is dependent on the 2nd provided assembly</returns>
        public static bool IsDependentOnOtherAssembly(this Assembly assembly, Assembly otherAssembly) =>
            assembly.IsDependentOnOtherAssembly(otherAssembly.FullName);
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks its references for a match with some other provided assembly
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking for a dependency</param>
        /// <param name="otherAssembly">The <see cref="AssemblyName"/> you are looking for a dependency for</param>
        /// <returns>true: if this assembly is dependent on the 2nd provided assembly</returns>
        public static bool IsDependentOnOtherAssembly(this Assembly assembly, AssemblyName otherAssembly) =>
            assembly.IsDependentOnOtherAssembly(otherAssembly.FullName);
        /// <summary>
        /// Takes an <see cref="Assembly"/> and checks its references for a match with some other provided assembly full name
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> you're checking for a dependency</param>
        /// <param name="otherAssemblyFullName">The full name of the assembly you are looking for a dependency for</param>
        /// <returns>true: if this assembly is dependent on the 2nd provided assembly</returns>
        public static bool IsDependentOnOtherAssembly(this Assembly assembly, string otherAssemblyFullName)
        {
            //Use the assembly if it references the RivenFramework
            foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                if (referencedAssembly.FullName == otherAssemblyFullName)
                    return true;

            //Don't use any assembly that doesn't pass the above checks
            return false;
        }

        #endregion
    }
}
