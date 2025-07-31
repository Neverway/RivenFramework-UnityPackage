using RivenFramework.Utils.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static EasyPropertyDrawer;

namespace RivenFramework
{
    public class TodoWindow : EditorWindow
    {
        private static TodoItemsDisplay itemsDisplay;
        public static string[] foundTags;
        [InvokeOnReflectionCacheLoad]
        public static void RefreshItemsDisplay(Filters filters) => itemsDisplay = new TodoItemsDisplay(filters);
        public static float windowWidth;   

        private Vector2 scrollViewPos = Vector2.zero;
        private bool someFoldout;

        private string[] enumeratedItems;
        private IEnumerable<string> enumerator;

        public Filters currentFilters = new Filters();

        public struct Filters
        {
            public string forWhoSearch;
        }

        private void OnGUI()
        {
            if (itemsDisplay == null)
                RefreshItemsDisplay(currentFilters);

            windowWidth = position.width;

            new TextField(currentFilters.forWhoSearch, (input) => 
            {
                currentFilters.forWhoSearch = input;
                RefreshItemsDisplay(currentFilters);
            });

            windowWidth -= 16f; //scrollbar
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            
            itemsDisplay.GetDrawerObject(currentFilters).Draw();

            GUILayout.EndScrollView();
            return;
        }

        private class TodoItemsDisplay
        {
            private NamespaceGroup noNamespaceGroup = new(null);
            private Dictionary<string, NamespaceGroup> namespaceGroups = new();

            public TodoItemsDisplay() : this(new Filters()) { }
            public TodoItemsDisplay(Filters filters)
            {
                AttributeUsage[] allToDos = ReflectionCache.GetAttributeUsages<TodoAttribute>();

                foreach (AttributeUsage todo in allToDos)
                    Register(todo.As<TodoAttribute>(), todo.Member);
            }

            public void Register(TodoAttribute todoUsage, MemberInfo member)
            {
                //Get the associated Type of the member
                Type memberType;
                if (member is TypeInfo typeinfo)
                    memberType = typeinfo.AsType();
                else
                    memberType = member.ReflectedType;

                //Find the NamespaceGroup to register to
                string namespaceName = memberType.Namespace;
                NamespaceGroup targetGroup;
                if (string.IsNullOrEmpty(namespaceName)) //Use noNamespaceGroup if no namespace
                    targetGroup = noNamespaceGroup;
                else if (!namespaceGroups.TryGetValue(namespaceName, out targetGroup))
                {
                    targetGroup = new NamespaceGroup(namespaceName); //Create new if not found
                    namespaceGroups.Add(namespaceName, targetGroup);
                }

                //Register todo item to the associated NamespaceGroup
                targetGroup.Register(todoUsage, member, memberType);
            }

            public DrawerObject GetDrawerObject(Filters filters)
            {
                VerticalGroup contents = new VerticalGroup();

                if (noNamespaceGroup.typeGroups.Count > 0)
                    contents.Add(noNamespaceGroup.GetDrawerObject(filters));

                foreach (NamespaceGroup group in namespaceGroups.Values)
                    contents.Add(group.GetDrawerObject(filters));

                return contents; 
            }

            private class NamespaceGroup
            {
                public string identifier;
                public Dictionary<Type, TypeGroup> typeGroups = new();
                public bool foldout = true;
                public NamespaceGroup(string identifier) => this.identifier = identifier;

                public void Register(TodoAttribute todoUsage, MemberInfo member, Type memberType)
                {
                    TypeGroup targetGroup;
                    if (!typeGroups.TryGetValue(memberType, out targetGroup))
                    {
                        targetGroup = new TypeGroup(memberType); //Create new if not found
                        typeGroups.Add(memberType, targetGroup);
                    }

                    targetGroup.Register(todoUsage, member);
                }

                public DrawerObject GetDrawerObject(Filters filters)
                {
                    string titleLabel = string.IsNullOrEmpty(identifier) ? "No Namespace" : identifier;

                    VerticalGroup title = new VerticalGroup();
                    title.Add(new Label(titleLabel).Big().Bold().AlignCenter().AlignLower());
                    title.Add(new Divider().Padding(-2f, 1f));

                    VerticalGroup contents = new VerticalGroup();
                    foreach (TypeGroup typeGroup in typeGroups.Values)
                        contents.Add(typeGroup.GetDrawerObject(filters));

                    VerticalGroup toReturn = new VerticalGroup();
                    toReturn.Add(new EmptySpace(10));
                    toReturn.Add(new Foldout(foldout, SetFoldout, title, contents));
                    return toReturn;
                }

                public void SetFoldout(bool newFoldout) => this.foldout = newFoldout;
            }
            private class TypeGroup
            {
                private Type identifier;
                private MemberGroup classTypeGroup;
                private Dictionary<MemberInfo, MemberGroup> memberGroups = new();

                public TypeGroup(Type identifier) => this.identifier = identifier;

                public void Register(TodoAttribute todoUsage, MemberInfo member)
                {
                    MemberGroup targetGroup;
                    if (member is TypeInfo typeInfo)
                    {
                        if (classTypeGroup == null)
                            classTypeGroup = new MemberGroup(typeInfo);
                        targetGroup = classTypeGroup;
                    }
                    else if (!memberGroups.TryGetValue(member, out targetGroup))
                    {
                        targetGroup = new MemberGroup(member); //Create new if not found
                        memberGroups.Add(member, targetGroup);
                    }

                    targetGroup.Register(todoUsage);
                }

                public DrawerObject GetDrawerObject(Filters filters)
                {
                    DrawerObject title = new Label(GetTitle()).UseRichText();

                    if (classTypeGroup != null)
                    {
                        Label inlineDescription = new Label(
                            GetTitle() + classTypeGroup.GetFullTodo());
                        inlineDescription.UseRichText();
                        if (inlineDescription.GetLabelWidth() < windowWidth)
                            title = inlineDescription;
                        else
                            title = new VerticalGroup(title, classTypeGroup.GetDrawerObject(filters));
                    }
                    Button jumpToCodeButton = new Button("", () => { TestJumpToCode(identifier); });
                    title = new SizedHorizontalGroup(title).AddOnLeft(jumpToCodeButton, 16);
                    

                    VerticalGroup contents = new VerticalGroup();
                    foreach (MemberGroup typeGroup in memberGroups.Values)
                        contents.Add(typeGroup.GetDrawerObject(filters));

                    return new VerticalGroup(title, contents.AddIndent().AddIndent());
                }

                private string GetTitle()
                {
                    string typeColor = "4EC9AD";

                    if (identifier.IsInterface || identifier.IsEnum)
                        typeColor = "ABD7A3";

                    return $"<color=#{typeColor}><size=14><b>{identifier.NameWithGenericAndArray()}</b></size></color>";
                }
            }
            private class MemberGroup
            {
                private MemberInfo identifier;
                private List<string> todoItems = new();

                public MemberGroup(MemberInfo identifier) => this.identifier = identifier;

                public void Register(TodoAttribute todoUsage)
                {
                    todoItems.Add(todoUsage.GetDescription());
                }

                public DrawerObject GetDrawerObject(Filters filters)
                {
                    string todoText = GetFullTodo();
                    VerticalGroup contents = new VerticalGroup();
                    Button jumpToCodeButton = new Button("", () => { TestJumpToCode(identifier); }).AsToggle();
                    
                    if (identifier is not TypeInfo)
                    {
                        Label inlineDescription = new Label(GetTitle() + todoText);
                        inlineDescription.UseRichText();

                        if (inlineDescription.GetLabelWidth() < windowWidth - 40)
                            return contents.Add(new SizedHorizontalGroup(inlineDescription).AddOnLeft(jumpToCodeButton, 16));

                        inlineDescription = new Label(GetTitle());
                        inlineDescription.UseRichText();
                        contents.Add(new SizedHorizontalGroup(inlineDescription).AddOnLeft(jumpToCodeButton, 16));
                    }

                    VerticalGroup todoContent = new VerticalGroup();
                    contents.Add(new Label(GetFullTodo()).CalcLabelLinesFromWidth(windowWidth - 40).WordWrap().UseRichText());
                    contents.Add(todoContent.AddIndent().AddIndent());

                    return contents;
                }

                public string GetFullTodo()
                {
                    StringBuilder sb = new StringBuilder();

                    bool first = true;
                    foreach (string todoItem in todoItems)
                    {
                        if (!first)
                            sb.Append("  |  ");
                        sb.Append(todoItem);
                        first = false;
                    }

                    return $"   -  <size=10><i>{sb}</i></size>";
                }

                private string GetTitle()
                {
                    string color = "DCDCDC";
                    if (identifier is MethodInfo)
                        color = "DCDCAA";

                    string memberName;
                    if (identifier is TypeInfo)
                        memberName = "SHOULDNT BE TYPEINFO";
                    else if (identifier is ConstructorInfo)
                    {
                        memberName = $"{identifier.ReflectedType}(...)";
                        color = "4EC9AD";
                    }
                    else if (identifier is MethodInfo)
                    {
                        memberName = $".{identifier.Name}(...)";
                        color = "DCDCAA";
                    }
                    else
                        memberName = $".{identifier.Name}";

                    return $"<color=#{color}><b>{memberName}</b></color>";
                }
            }
        }

        [MenuItem("Neverway/Todo List")]
        public static void ShowWindow()
        {
            GetWindow<TodoWindow>("My Window");
        }

        public static void TestJumpToCode(MemberInfo memberToJumpTo)
        {
            Tuple<string, int> location = GetScriptFile(memberToJumpTo);
            if (location == null)
            {
                Debug.LogWarning("Could not find location to member");
                return;
            }
            // Open the file in the external script editor at the specific line
            InternalEditorUtility.OpenFileAtLineExternal(location.Item1, location.Item2);
        }
        public static Tuple<string, int> GetScriptFile(MemberInfo memberToJumpTo)
        {
            string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript");

            foreach (string guid in scriptGuids)
            {
                try {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScriptReader reader = new MonoScriptReader(path);
                    MonoScriptReader.TokenInfo token = reader.GetMemberInfoTokenImmediate(memberToJumpTo);
                    if (token.charPosition != 0)
                        return new Tuple<string, int>(path, token.linePosition);
                } 
                catch (Exception e) 
                { 
                    if (e is not DirectoryNotFoundException)
                        Debug.LogException(e); 
                }
            }
            return null;
        }

        private class TodoTypeCategory
        {
            public List<AttributeUsage> typeUsages = new List<AttributeUsage>();
            public List<TodoMemberCategory> memberCategories = new List<TodoMemberCategory>();
        }

        private class TodoMemberCategory
        {
            public List<AttributeUsage> todoUsages = new List<AttributeUsage>();
        }


        
    }
}
