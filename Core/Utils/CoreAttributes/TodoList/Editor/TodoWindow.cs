using RivenFramework.Utils.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.InputField;


//EasyPropertyDrawer conflicts with UnityEngine.UIElements,
//so I am forced to use "EasyPropertyDrawer" name to reference its classes, which is long,
//so I'll use "EZ" as a shorthand for "EasyPropertyDrawer"
using EZ = EasyPropertyDrawer; 

namespace RivenFramework
{
    public class TodoWindow : EditorWindow
    {

        [Todo_ToRemove(Owner = "errynei")]
        private enum TestTodos
        {
            [Todo("Minor severity test!", TodoSeverity.Minor)]
            MinorSeverityTest,
            [Todo("Moderate severity test!", TodoSeverity.Moderate)]
            ModerateSeverityTest,
            [Todo("Major severity test!", TodoSeverity.Major)]
            MajorSeverityTest,
            [Todo("CRITICAL severity test! AAAAH!", TodoSeverity.Critical)]
            CRITICALSeverityTest,

            [Todo_AddComments(severity: TodoSeverity.Minor)]
            [Todo_Optimize(severity: TodoSeverity.Moderate)]
            [Todo_ToRemove(severity: TodoSeverity.Major)]
            [Todo_StressTest(severity: TodoSeverity.Critical)]
            MultipleTodoTest
        }



        /// <summary>Template UI asset for the whole window to be cloned and queried for UI components to 
        /// define functionality for</summary>
        [SerializeField] private VisualTreeAsset EUITemplate_TodoWindow;

        

        [Todo_Implement(Owner = "Errynei")]
        public List<SearchFilter> SearchFilters { 
            get 
            {
                return new List<SearchFilter>();
            } 
        }

        [MenuItem("Neverway/Todo List")]
        public static void ShowWindow() => GetWindow<TodoWindow>("Todo List");

        //Called by Unity to set up rootVisualELement which contains all window information
        private void CreateGUI()
        {
            //Attempt to clone the UI template for the rootVisualElement 
            if (CloneEUITemplateToRootVisualElement())
                //if successful, pull VisualElements from the cloned template and setup their functions
                SetupAllVisualElements(); 
            else
                //if not successful, fallback on old IMGUI display of ToDo items (There will be no search functionality)
                UseIMGUIContainerTodoListForRootVisualElement();
        }

        #region VisualElement initialization setup and references

        /// <summary>Contains all visual information for the window. Cloned from <see cref="EUITemplate_TodoWindow"/>.
        /// <br/>Is used to pull certain <see cref="VisualElement"/>s from to set up the window's functionality.</summary>
        private VisualElement VE_RootWindowContainer;

        ///<summary>Clones <c>EUITemplate_TodoWindow</c> to the <c>rootVisualElement</c> and stores that clone 
        ///as <see cref="VE_RootWindowContainer"/></summary>
        private bool CloneEUITemplateToRootVisualElement()
        {
            //If there is no UI Template for the window, display a warning and return false for a failure
            if (EUITemplate_TodoWindow == null)
            {
                Debug.LogWarning($"{nameof(TodoWindow)}: UXML asset ({nameof(VisualTreeAsset)}) was not found. " +
                        $"Cannot draw search settings for window. To fix this, go to the location of this script " +
                        $"(which is provided as context for this warning), click on it, and make sure in the inspector " +
                        $"that {nameof(EUITemplate_TodoWindow)} is filled with the appropriate UXML asset " +
                        $"(should be in the same folder).", MonoScript.FromScriptableObject(this));
                return false;
            }
            //Try to clone the UI template, store reference to it as root window container, and set as only item in rootVisualElement
            try
            {
                rootVisualElement.Clear();
                VE_RootWindowContainer = EUITemplate_TodoWindow.CloneTree();
                rootVisualElement.Add(VE_RootWindowContainer);
                return true;
            }
            catch (Exception e)
            {
                //Log error if failed, this should never happen
                VE_RootWindowContainer = null;
                Debug.LogError($"{nameof(TodoWindow)}: Was not able to clone {nameof(EUITemplate_TodoWindow)} for " +
                    $"{VE_RootWindowContainer} for some reason? Exception:\n{e.Message}", 
                    MonoScript.FromScriptableObject(this));
                return false;
            }
        }

        /// <summary>Instantiates a <see cref="IMGUIContainer"/> for <c>rootVisualElement</c> and provides it with draw function for
        /// the todo list just as it would have for the <see cref="IMGUIContainer"/> inside of the UI template 
        /// <see cref="EUITemplate_TodoWindow"/>.<br/>This is just a fallback in case there was no UI template provided,
        /// there will be no search functions if this is used</summary>
        private void UseIMGUIContainerTodoListForRootVisualElement()
        {
            rootVisualElement.Clear();
            VE_ListContainer = new IMGUIContainer(() => TodoItemsDisplay.DrawInstance(position));
            rootVisualElement.Add(VE_ListContainer);
        }
        
        /// <summary>Pulls a <see cref="VisualElement"/> of a certain type from <see cref="EUITemplate_TodoWindow"/> 
        /// based on a string tag. <br/>Logs an error if it was unsuccessful, which may be due to wrong UI Template</summary>
        private bool FindVisualElement<T>(string id, out T visualElement) where T : VisualElement
        {
            //Query the root window container
            visualElement = VE_RootWindowContainer.Q<T>(id);
            //Log error if there was none found
            bool success = visualElement != null;
            if (!success) Debug.LogError($"{nameof(TodoWindow)}: could not find {nameof(VisualElement)} " +
                $"of type \'{typeof(T).Name}\' by tag \"{id}\". \nMake sure the tag const is correct and the " +
                $"assigned UXML asset \'{EUITemplate_TodoWindow.name}\' contains a {nameof(VisualElement)} " +
                $"of that type by that tag.", EUITemplate_TodoWindow);

            //Return whether or not a VisualElement was found
            return success;
        }

        /// <summary>Calls all methods to define window functionality (which should all be located below this function)</summary>
        /// <exception cref="NullReferenceException"></exception>
        private void SetupAllVisualElements()
        {
            //Check if the root window container has not been set up. This should not ever happen so throw an error
            if (VE_RootWindowContainer == null)
                throw new NullReferenceException($"{nameof(TodoWindow)}: {nameof(VE_RootWindowContainer)} is null. " +
                    $"Make sure you are calling {nameof(CloneEUITemplateToRootVisualElement)}() before calling " +
                    $"{nameof(SetupAllVisualElements)} to initialize {nameof(VE_RootWindowContainer)} (since that is " +
                    $"where all the {nameof(VisualElement)}s in this window pull their references from for setup).\n");

            //Call all setup functions for each visual element that we define functionality for
            Setup_VE_ListContainer();
            Setup_VE_SearchOwnerDropdown();
            Setup_VE_SearchOwnerToggle();
            Setup_VE_SearchSeverityDropdown();
            Setup_VE_SearchSeverityToggle();
        }


        /// <summary>Container for displaying all of the TodoAttributes in a nice organized list with JumpTo buttons</summary>
        private IMGUIContainer VE_ListContainer;
        private const string ID_VE_LIST_CONTAINER = "List_Container";
        /// <summary>Sets up functionality of <see cref="VE_ListContainer"/></summary>
        private void Setup_VE_ListContainer()
        {
            //Get List Container from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_LIST_CONTAINER, out VE_ListContainer)) return;
            //setup gui of IMGUI container to call the draw function of the todo List
            VE_ListContainer.onGUIHandler = () => TodoItemsDisplay.DrawInstance(position);
        }


        //Search option: Dropdown for selecting which owner to search for
        private DropdownField VE_SearchOwnerDropdown;
        private const string ID_VE_SEARCH_OWNER_DROPDOWN = "Search_Owner_Dropdown";
        private const string PREFS_SEARCH_OWNER_DROPDOWN = "RivenFramework.TodoWindow.SearchOwnerDropdown";
        /// <summary>Sets up functionality of <see cref="VE_SearchOwnerDropdown"/></summary>
        private void Setup_VE_SearchOwnerDropdown()
        {
            //Get List Container from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_SEARCH_OWNER_DROPDOWN, out VE_SearchOwnerDropdown)) return;

            //Set dropdown choices to each unique occurance of an owner across all TodoAttributes
            VE_SearchOwnerDropdown.choices = TodoItemsDisplay.GetOwnerOptions();

            //Setup to work with EditorPrefs to save settings per user
            string prefsSetting = EditorPrefs.GetString(PREFS_SEARCH_OWNER_DROPDOWN, VE_SearchOwnerDropdown.choices[0]);
            if (!VE_SearchOwnerDropdown.choices.Contains(prefsSetting)) //Fallback to default setting if prefs setting is no longer in options
                prefsSetting = VE_SearchOwnerDropdown.choices[0];
            VE_SearchOwnerDropdown.SetValueWithoutNotify(prefsSetting);
            VE_SearchOwnerDropdown.RegisterValueChangedCallback(changeEvent =>
            {
                EditorPrefs.SetString(PREFS_SEARCH_OWNER_DROPDOWN, changeEvent.newValue);
                OnUpdatedSearch();
            });
        }


        //Search option: Toggle for showing TodoAttributes with no assigned owner along with 
        private Toggle VE_SearchOwnerToggle;
        private const string ID_VE_SEARCH_OWNER_TOGGLE = "Search_Owner_IncludeUnowned";
        private const string PREFS_SEARCH_OWNER_TOGGLE = "RivenFramework.TodoWindow.SearchOwnerToggle";
        /// <summary>Sets up functionality of <see cref="VE_SearchOwnerToggle"/></summary>
        private void Setup_VE_SearchOwnerToggle()
        {
            //Get "Search Severity Dropdown" from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_SEARCH_OWNER_TOGGLE, out VE_SearchOwnerToggle)) return;

            //Setup to work with EditorPrefs to save settings per user
            VE_SearchOwnerToggle.SetValueWithoutNotify(EditorPrefs.GetBool(PREFS_SEARCH_OWNER_TOGGLE, true));
            VE_SearchOwnerToggle.RegisterValueChangedCallback(changeEvent =>
            {
                EditorPrefs.SetBool(PREFS_SEARCH_OWNER_TOGGLE, changeEvent.newValue);
                OnUpdatedSearch();
            });
        }

        //Search option: Dropdown for selecting which minimum severity to search for
        private EnumField VE_SearchSeverityDropdown;
        private const string ID_VE_SEARCH_SEVERITY_DROPDOWN = "Search_Severity_Dropdown";
        private const string PREFS_SEARCH_SEVERITY_DROPDOWN = "RivenFramework.TodoWindow.SearchSeverityDropdown";
        /// <summary>Sets up functionality of <see cref="VE_SearchSeverityDropdown"/></summary>
        private void Setup_VE_SearchSeverityDropdown()
        {
            //Get "Search Severity Dropdown" from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_SEARCH_SEVERITY_DROPDOWN, out VE_SearchSeverityDropdown)) return;

            //Setup to work with EditorPrefs to save settings per user
            VE_SearchSeverityDropdown.Init((TodoSeverity)
                EditorPrefs.GetInt(PREFS_SEARCH_SEVERITY_DROPDOWN, (int)TodoSeverity.Moderate));
            VE_SearchSeverityDropdown.RegisterValueChangedCallback(changeEvent =>
            {
                EditorPrefs.SetInt(PREFS_SEARCH_SEVERITY_DROPDOWN, (int)(TodoSeverity)changeEvent.newValue);
                OnUpdatedSearch();
            });
        }

        //Search option: Toggle for choosing if severities of higher levels should be shown
        private Toggle VE_SearchSeverityToggle;
        private const string ID_VE_SEARCH_SEVERITY_TOGGLE = "Search_Severity_IncludeHigher";
        private const string PREFS_SEARCH_SEVERITY_TOGGLE = "RivenFramework.TodoWindow.SearchSeverityToggle";
        /// <summary>Sets up functionality of <see cref="VE_SearchSeverityToggle"/></summary>
        private void Setup_VE_SearchSeverityToggle()
        {
            //Get "Search Severity Dropdown" from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_SEARCH_SEVERITY_TOGGLE, out VE_SearchSeverityToggle)) return;

            //Setup to work with EditorPrefs to save settings per user
            VE_SearchSeverityToggle.SetValueWithoutNotify(
                EditorPrefs.GetBool(PREFS_SEARCH_SEVERITY_TOGGLE, true));
            VE_SearchSeverityToggle.RegisterValueChangedCallback(changeEvent =>
            {
                EditorPrefs.SetBool(PREFS_SEARCH_SEVERITY_TOGGLE, changeEvent.newValue);
                OnUpdatedSearch();
            });
        }
        #endregion

        public void OnUpdatedSearch()
        {
            TodoItemsDisplay.RefreshInstance(GetSearchFilters());
        }
        public List<SearchFilter> GetSearchFilters()
        {
            if (VE_RootWindowContainer == null) return new List<SearchFilter>();

            string searchOwner = "All";
            if (VE_SearchOwnerDropdown != null) searchOwner = VE_SearchOwnerDropdown.value;
            bool searchUnowned = true;
            if (VE_SearchOwnerToggle != null) searchUnowned = VE_SearchOwnerToggle.value;
            TodoSeverity searchSeverity = TodoSeverity.Moderate;
            if (VE_SearchSeverityDropdown != null) searchSeverity = (TodoSeverity)VE_SearchSeverityDropdown.value;
            bool searchHigherSeverity = true;
            if (VE_SearchSeverityToggle != null) searchHigherSeverity = VE_SearchSeverityToggle.value;

            List<SearchFilter> searchFilters = new List<SearchFilter>();
            searchFilters.Add(new OwnerFilter(searchOwner, searchUnowned));
            searchFilters.Add(new SeverityFilter(searchSeverity, searchHigherSeverity));
            return searchFilters;
        }

        /// <summary>Returns <see cref="DrawerObject"/> for button that opens your IDE to provided 
        /// <see cref="TodoAttribute"/></summary>
        public static EZ.DrawerObject GetOpenCodeButton(TodoAttribute toJumpTo)
        {
            if (toJumpTo == null)
                return new EZ.EmptySpace();

            /* Me just messing around with icons:
                ♜♝♞♛♚♞♝♜
                ♟♟♟♟♟♟♟♟




                ♙♙♙♙♙♙♙♙
                ♖♗♘♕♔♘♗♖

                🕷⏻⚐⚑⌬ */

            bool useJokeIcon = false; //UnityEngine.Random.value < 0.01f;

            GUIStyle buttonStyle = EditorStyles.iconButton;
            buttonStyle.fontSize = useJokeIcon ? 18 : 13; //↩⟵✏

            return new EZ.Button(useJokeIcon ? "☭" : "↩",
                            () => { toJumpTo.EDITOR_OpenFileAtAttributeLocation(); })
                            .SetStyle(buttonStyle);
        }

        private class TodoItemsDisplay
        {
            private NamespaceGroup noNamespaceGroup = new(null);
            private Dictionary<string, NamespaceGroup> namespaceGroups = new();
            private Vector2 scrollViewPos = Vector2.zero;
            public AttributeUsage[] AttributeUsages { get; private set; }
            public static float WindowWidth { get; private set; }
            private static TodoItemsDisplay itemsDisplayInstance;

            public static List<SearchFilter> searchFilters;

            public TodoItemsDisplay() : this(new List<SearchFilter>()) { }
            public TodoItemsDisplay(List<SearchFilter> filters)
            {
                searchFilters = filters;
                AttributeUsages = ReflectionCache.GetAttributeUsages<TodoAttribute>();

                foreach (AttributeUsage todo in AttributeUsages)
                {
                    TodoAttribute todoAtt = todo.As<TodoAttribute>();
                    bool passesFilter = true;
                    foreach (SearchFilter filter in searchFilters)
                        passesFilter &= filter.IncludeThroughFilter(todoAtt);

                    if (passesFilter)
                        Register(todoAtt, todo.Member);
                }
                    
            }

            /// <summary>Completely resets the <see cref="TodoItemsDisplay"/> instance by reinstantiating it</summary>
            [InvokeOnReflectionCacheLoad]
            public static void RefreshInstance() =>
                itemsDisplayInstance = new TodoItemsDisplay(new List<SearchFilter>());
            /// <summary>Completely resets the <see cref="TodoItemsDisplay"/> instance by reinstantiating it
            /// and allows passing in new search filters.<br/>Use this for updating the search filters</summary>
            public static void RefreshInstance(List<SearchFilter> newSearchFilters)
            {
                searchFilters = newSearchFilters;
                itemsDisplayInstance = new TodoItemsDisplay(searchFilters);
            }


            /// <summary>Creates an instance of <see cref="TodoItemsDisplay"/> if one does not exist
            /// and calls its <see cref="Draw"/> function</summary>
            public static void DrawInstance(Rect displayArea)
            {
                if (itemsDisplayInstance == null) 
                    RefreshInstance();

                itemsDisplayInstance.Draw(displayArea);
            }

            /// <summary>Returns a list of strings meant for the search options, with a string for each unique 
            /// <see cref="ToDoAttribute"/> owner including "All" as an option</summary>
            public static List<string> GetOwnerOptions()
            {
                //New empty options list, starting with the "All" option with a divider
                List<string> options = new List<string>() { "All", "Unowned", "" };
                foreach (AttributeUsage todo in itemsDisplayInstance.AttributeUsages)
                {
                    string currentTodoOwner = todo.As<TodoAttribute>().Owner;
                    if (!string.IsNullOrEmpty(currentTodoOwner)) //Only proceed if there is some kind of string
                    {
                        //Drop names to lowercase for consistency
                        currentTodoOwner = currentTodoOwner.ToLower();
                        //Add to options if this is a unique owner
                        if (!options.Contains(currentTodoOwner))
                            options.Add(currentTodoOwner);
                    }
                }
                return options;
            }

            public void Draw(Rect windowPosition)
            {
                WindowWidth = windowPosition.width;
                WindowWidth -= 16f;

                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
                EZ.DrawerObject itemsDisplayDrawerObject = null;

                //Try to get the drawerobject to draw for this todo list
                try { itemsDisplayDrawerObject = itemsDisplayInstance.GetDrawerObject(); }
                catch (Exception e)
                {
                    //If an error occurs, draw error message in window to avoid loads of errors printed to console every frame
                    GUIStyle richTextLabelStyle = new GUIStyle(EditorStyles.label);
                    richTextLabelStyle.richText = true;
                    richTextLabelStyle.wordWrap = true;
                    GUILayout.Label($"<color=#ff6666>Error trying to get {nameof(EZ.DrawerObject)}" +
                        $"for todo list\n{e}</color>", richTextLabelStyle);
                    if (GUILayout.Button("Press to output error to console"))
                        Debug.LogException(e);
                }
                //Draw drawerobject if getting it was a success
                if (itemsDisplayDrawerObject != null) itemsDisplayDrawerObject.Draw();

                GUILayout.EndScrollView();
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

            public EZ.DrawerObject GetDrawerObject()
            {
                EZ.VerticalGroup contents = new EZ.VerticalGroup();

                if (noNamespaceGroup.typeGroups.Count > 0)
                    contents.Add(noNamespaceGroup.GetDrawerObject());

                foreach (NamespaceGroup group in namespaceGroups.Values)
                    contents.Add(group.GetDrawerObject());

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

                public EZ.DrawerObject GetDrawerObject()
                {
                    string titleLabel = string.IsNullOrEmpty(identifier) ? "No Namespace" : identifier;

                    EZ.VerticalGroup title = new EZ.VerticalGroup();
                    title.Add(new EZ.Label(titleLabel).Big().Bold().AlignCenter().AlignLower());
                    title.Add(new EZ.Divider().Padding(-2f, 1f));

                    EZ.VerticalGroup contents = new EZ.VerticalGroup();
                    foreach (TypeGroup typeGroup in typeGroups.Values)
                        contents.Add(typeGroup.GetDrawerObject());

                    EZ.VerticalGroup toReturn = new EZ.VerticalGroup();
                    toReturn.Add(new EZ.EmptySpace(10));
                    toReturn.Add(new EZ.Foldout(foldout, SetFoldout, title, contents));
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
                            classTypeGroup = new MemberGroup(typeInfo, todoUsage);
                        targetGroup = classTypeGroup;
                    }
                    else if (!memberGroups.TryGetValue(member, out targetGroup))
                    {
                        targetGroup = new MemberGroup(member, todoUsage); //Create new if not found
                        memberGroups.Add(member, targetGroup);
                    }

                    targetGroup.Register(todoUsage);
                }

                public EZ.DrawerObject GetDrawerObject()
                {
                    //Initialize the basic title for this TypeGroup
                    EZ.DrawerObject title = new EZ.Label(GetTitle()).UseRichText();

                    //If a MemberGroup exists OF this TypeGroup, expand title to include todo task description with title
                    if (classTypeGroup != null)
                    {
                        EZ.Label inlineDescription = new EZ.Label(GetTitle() + classTypeGroup.GetFullTodo());
                        inlineDescription.UseRichText();
                        if (inlineDescription.GetLabelWidth() < WindowWidth)
                            title = inlineDescription;
                        else
                            title = new EZ.VerticalGroup(title, classTypeGroup.GetDrawerObject());
                    }

                    //Add button to jump to the attribute (will be an empty space if attribute is null)
                    EZ.DrawerObject jumpToCodeButton = GetOpenCodeButton(classTypeGroup?.firstFoundTodoAttribute);
                    title = new EZ.SizedHorizontalGroup(title).AddOnLeft(jumpToCodeButton, 16);

                    //Group all MemberGroups associated with this Type
                    EZ.VerticalGroup contents = new EZ.VerticalGroup();
                    foreach (MemberGroup typeGroup in memberGroups.Values)
                        contents.Add(typeGroup.GetDrawerObject());

                    //Return final DrawerObject to draw this TypeGroup which includes title and associated MemberGroups
                    return new EZ.VerticalGroup(title, contents.AddIndent().AddIndent()).AddIndent(-5);
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
                public TodoAttribute firstFoundTodoAttribute;
                private MemberInfo identifier;
                private List<string> todoItems = new();

                public MemberGroup(MemberInfo identifier, TodoAttribute todoAttribute)
                {
                    this.identifier = identifier;
                    this.firstFoundTodoAttribute = todoAttribute;
                }

                public void Register(TodoAttribute todoUsage)
                {
                    todoItems.Add(todoUsage.RichTextDescription);
                }

                public EZ.DrawerObject GetDrawerObject()
                {
                    string todoText = GetFullTodo();
                    EZ.VerticalGroup contents = new EZ.VerticalGroup();
                    EZ.DrawerObject jumpToCodeButton = GetOpenCodeButton(firstFoundTodoAttribute);

                    if (identifier is not TypeInfo)
                    {
                        EZ.Label inlineDescription = new EZ.Label(GetTitle() + todoText);
                        inlineDescription.UseRichText();

                        if (inlineDescription.GetLabelWidth() < WindowWidth - 40)
                            return contents.Add(new EZ.SizedHorizontalGroup(inlineDescription).AddOnLeft(jumpToCodeButton, 16));

                        inlineDescription = new EZ.Label(GetTitle());
                        inlineDescription.UseRichText();
                        contents.Add(new EZ.SizedHorizontalGroup(inlineDescription).AddOnLeft(jumpToCodeButton, 16));
                    }

                    EZ.VerticalGroup todoContent = new EZ.VerticalGroup();
                    contents.Add(new EZ.Label(GetFullTodo()).CalcLabelLinesFromWidth(WindowWidth - 40).WordWrap().UseRichText());
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

        public abstract class SearchFilter { public abstract bool IncludeThroughFilter(TodoAttribute attribute); }
        public class SeverityFilter : SearchFilter
        {
            TodoSeverity severity;
            bool includeHigher;
            public SeverityFilter(TodoSeverity severity, bool includeHigher)
            {
                this.severity = severity;
                this.includeHigher = includeHigher;
            }

            public override bool IncludeThroughFilter(TodoAttribute attribute)
            {
                if (includeHigher)
                {
                    return ((int)attribute.Severity) >= ((int)severity);
                }
                else
                {
                    return attribute.Severity == severity;
                }
            }
        }
        public class OwnerFilter : SearchFilter
        {
            string ownerFilter;
            bool includeUnowned;
            bool includeAll;
            public OwnerFilter(string ownerFilter, bool includeUnowned)
            {
                this.includeAll = ownerFilter == "All";
                this.ownerFilter = ownerFilter;
                this.includeUnowned = includeUnowned || ownerFilter == "Unowned";
            }

            public override bool IncludeThroughFilter(TodoAttribute attribute)
            {
                if (includeAll) 
                    return true;
                if (includeUnowned && string.IsNullOrEmpty(attribute.Owner))
                    return true;

                string owner = attribute.Owner;
                if (owner == null) owner = "";
                owner = owner.ToLower();

                return ownerFilter == owner;
            }
        }

    }
}
