using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RivenFramework.Utils.Reflection
{
    public class ReflectionCache
    {
        internal static ReflectionCache _instance;
        
        public static ReflectionCache Instance { get =>
                _instance == null ? _instance = new ReflectionCache() : _instance;}

        private HashSet<Assembly> cachedAssemblies = new HashSet<Assembly>();

        private Dictionary<Type, MemberInfo[]> typeToMemberInfos = new();
        private Dictionary<MemberInfo, List<AttributeUsage>> memberInfoToAttributeUsages = new();
        private Dictionary<Type, List<AttributeUsage>> attributeTypeToAttributeUsages = new();

        #if UNITY_EDITOR
        private Dictionary<Type, MonoScript[]> typeToMonoScript = new();
        #endif

        private const BindingFlags getMembersBindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        internal static void Reset() =>
            _instance = new ReflectionCache();

        public ReflectionCache()
        {
            //Benchmark.StartTiming();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                //assembly.DEBUG_PrintFolderLocation(assembly.IsProjectScriptAssembly() && !assembly.IsUnityAssembly());
                if (assembly.IsProjectScriptAssembly() && !assembly.IsUnityAssembly())
                    AddAssemblyToCache(assembly);
            }
            //Benchmark.StopTiming("Reflection Cache");
        }

        public void AddAssemblyToCache(Assembly assembly)
        {
            //Do not cache the assembly if it has already been cached
            if (cachedAssemblies.Contains(assembly)) return;
            cachedAssemblies.Add(assembly);

            //Add all types from the assembly into the cache
            Type[] assemblyTypes = assembly.GetTypes();
            foreach (Type type in assemblyTypes)
                AddTypeToCache(type);
        }

        public void AddTypeToCache(Type type)
        {
            //Do not cache the type if it has already been cached
            if (typeToMemberInfos.ContainsKey(type)) return;

            //Get all MemberInfos from this Type, and cache it into Type to MemberInfos Dictionary
            MemberInfo[] members = type.GetMembers(getMembersBindingFlags);
            typeToMemberInfos.Add(type, members);

            //Cache this Type's TypeInfo as well as all of its contained MemberInfos
            AddMemberToCache(type.GetTypeInfo());
            foreach (MemberInfo member in typeToMemberInfos[type])
                AddMemberToCache(member);
        }
    
        public void AddMemberToCache(MemberInfo member)
        {
            //Do not cache the member if it has already been cached
            if (memberInfoToAttributeUsages.ContainsKey(member)) return;

            //Get all attributes from the member
            IEnumerable<Attribute> allAttributes = member.GetCustomAttributes<Attribute>(inherit: true);
            //If there is not any attributes, add it to the dictionary to mark as being cached but with null value
            if (!allAttributes.Any())
            {
                memberInfoToAttributeUsages.Add(member, null);
                return;
            }
            //Otherwise, proceed by starting with an empty list for the cached AttributeUsages
            List<AttributeUsage> memberAttributeUsages = new List<AttributeUsage>();
            memberInfoToAttributeUsages.Add(member, memberAttributeUsages);

            //Also get base attributes for later to compare against allAttributes to setup AttributeUsage.IsInhereted later
            IEnumerable<Attribute> baseAttributes = member.GetCustomAttributes<Attribute>(inherit: false);

            //Create an AttributeUsage instance for each Attribute on this member
            foreach (Attribute attribute in allAttributes)
            {
                //This attribute is an inhereted one if it is NOT also contained in baseAttributes
                bool isInherited = !baseAttributes.Contains(attribute);

                //Create an AttributeUsage instance, and add it to the cached list of this members attribute usages
                AttributeUsage currentUsage = new AttributeUsage(member, attribute, isInherited);
                memberAttributeUsages.Add(currentUsage);

                //Add this AttributeUsage to the AttributeType to AttributeUsages Dictionary 
                CacheAttributeTypeToAttributeUsages(currentUsage);
            }
        }

        private void CacheAttributeTypeToAttributeUsages(AttributeUsage attributeUsage)
        {
            foreach (Type subType in attributeUsage.Type.GetAllTypesAssignableTo())
            {
                //If this Attribute Type is not yet in the Dictionary, add a spot for it with an empty List
                if (!attributeTypeToAttributeUsages.ContainsKey(subType))
                    attributeTypeToAttributeUsages.Add(subType, new List<AttributeUsage>());

                //Add the AttributeUsage to the current type's spot in the Attribute Type to AttributeUsages Dictionary
                attributeTypeToAttributeUsages[subType].Add(attributeUsage);
            }
        }


        public MemberInfo[] GetMemberInfosFromType(Type type)
        {
            //Retrieve from the cache the MemberInfos associated with the given Type (Throw Exception if not found)
            MemberInfo[] members;
            if (!typeToMemberInfos.TryGetValue(type, out members))
                throw new ArgumentOutOfRangeException($"The Type {type.Name}" +
                    $" has NOT been cached in ReflectionCache. Call caching methods to add to cache first");

            //Return the members as a newly constructed copy of the array to avoid editing of the cached data
            return members.ToArray();
        }
        public AttributeUsage[] GetMemberInfoAttributeUsages(MemberInfo member)
        {
            //Retrieve from the cache the AttributeUsages associated with the given MemberInfo (Throw Exception if not found)
            List<AttributeUsage> attributeUsages;
            if (!memberInfoToAttributeUsages.TryGetValue(member, out attributeUsages))
                throw new ArgumentOutOfRangeException($"The MemberInfo {member.DeclaringType}.{member.Name}" +
                    $" has NOT been cached in ReflectionCache. Call caching methods to add to cache first");

            //If there was no list, there was no attributes, return an empty array
            if (attributeUsages == null)
                return new AttributeUsage[0];

            //Return the list as an array to avoid the cached list from being edited
            return attributeUsages.ToArray();
        }
        public AttributeUsage[] GetAttributeUsagesFromAttributeType(Type type)
        {
            //Validate arguments (type cannot be null, and must be extended from Attribute
            if (type == null)
                throw new ArgumentNullException("Given type cannot be null");
            if (!typeof(Attribute).IsAssignableFrom(type))
                throw new ArgumentException("Given type must be of type Attribute");
            if (type == typeof(Attribute))
                throw new NotImplementedException("Retrieving AttributeUsages from type \'Attribute\' is not implemented");

            //Retrieve from the cache the AttributeUsages associated with the given Attribute Type (return empty array if not found)
            List<AttributeUsage> attributeUsages;
            if (!attributeTypeToAttributeUsages.TryGetValue(type, out attributeUsages))
                return new AttributeUsage[0];

            //If there was no list, there was no attributes, return an empty array
            if (attributeUsages == null)
                return new AttributeUsage[0];

            //Return the list as an array to avoid the cached list from being edited
            return attributeUsages.ToArray();
        }

        public static AttributeUsage[] GetAttributeUsages<TAttribute>() 
            where TAttribute : Attribute
            => Instance.GetAttributeUsagesFromAttributeType(typeof(TAttribute));
    }

    public static class ReflectionCacheExtensionMethods
    {
        public static bool HasAttribute<TType>(this MemberInfo member, bool inherit)
            where TType : class
        {
            //todo: What happens when base type attribute is provided like "Attribute" for TType
            foreach (AttributeUsage usage in member.GetCachedAttributeUsages())
            {
                if (usage.Is<TType>())
                {
                    if (!inherit && usage.IsInherited)
                        continue;

                    return true;
                }
            }
            return false;
        }
        public static bool HasAttribute<TType>(this MemberInfo member, out TType attribute, bool inherit)
            where TType : class
        {
            //todo: What happens when base type attribute is provided like "Attribute" for TType
            foreach(AttributeUsage usage in member.GetCachedAttributeUsages())
            {
                if (usage.Is<TType>())
                {
                    if (!inherit && usage.IsInherited)
                        continue;

                    attribute = usage.As<TType>();
                    return true;
                }
            }
            attribute = null;
            return false;
        }
        public static IEnumerable<TType> GetAttributes<TType>(this MemberInfo member, bool inherit)
        {
            foreach (AttributeUsage usage in member.GetCachedAttributeUsages())
            {
                if (usage.Is<TType>())
                {
                    if (!inherit && usage.IsInherited)
                        continue;

                    yield return usage.As<TType>();
                }
            }
        }

        public static MemberInfo[] GetCachedMemberInfos(this Type type) =>
            ReflectionCache.Instance.GetMemberInfosFromType(type);

        public static AttributeUsage[] GetCachedAttributeUsages(this MemberInfo memberInfo) =>
            ReflectionCache.Instance.GetMemberInfoAttributeUsages(memberInfo);
    }
}
