using RivenFramework.Utils.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RivenFramework.TodoWindow;



//EasyPropertyDrawer conflicts with UnityEngine.UIElements,
//so I am forced to use "EasyPropertyDrawer" name to reference its classes, which is long,
//so I'll use "EZ" as a shorthand for "EasyPropertyDrawer"
using EZ = EasyPropertyDrawer; 

namespace RivenFramework
{
    public class TodoWindow : EditorWindow
    {
        //Name and category hierarchy of where to show up in the top menu bar
        public const string MENU_ITEM_NAME = "Neverway/Todo List";

        /// <summary>Template UI asset for the whole window to be cloned and queried for UI components to 
        /// define functionality for</summary>
        [SerializeField] private VisualTreeAsset EUITemplate_TodoWindow;

        private AttributeUsage[] todoAttributes_all;
        private AttributeUsage[] todoAttributes_filtered;
        private List<SearchFilter> searchFilters;
        private List<string> ownerOptions;
        private TodoItemsDisplay todoListDisplay;


        [MenuItem(MENU_ITEM_NAME)]
        public static void ShowWindow() => GetWindow<TodoWindow>("Todo List");

        /// <summary>Called when any change happens to any values in search options UI. Also is called when ReflectionCache 
        /// reloads because thats after script recompile which may mean a change in TodoAttributes</summary>

        public void OnUpdatedSearch() => GenerateSearchFilters();

        [InvokeOnReflectionCacheLoad]
        public void OnRecompile() => GrabTodoAttributes();

        public void GrabTodoAttributes()
        {
            //Grab all attributes from the Reflection Cache
            todoAttributes_all = ReflectionCache.GetAttributeUsages<TodoAttribute>();
            //With the cached attributes refreshed, reapply the search filters
            ApplyFilterToTodoAttributes();
            //And regenerate the owner search options (There may be a new unique owner to search for)
            GenerateOwnerOptions();
        }
        public void GenerateSearchFilters()
        {
            //Clear the list of search filters
            searchFilters = new List<SearchFilter>();

            //Get all the values from the search options UI (or, if theyre not assigned, get the option that does not hide results)
            string searchOwner = VE_SearchOwnerDropdown == null ? "All" : VE_SearchOwnerDropdown.value;
            bool searchUnowned = VE_SearchOwnerToggle == null ? true : VE_SearchOwnerToggle.value;
            TodoSeverity searchSeverity = VE_SearchSeverityDropdown == null ? TodoSeverity.Minor : 
                (TodoSeverity)VE_SearchSeverityDropdown.value;
            bool searchHigherSeverity = VE_SearchSeverityToggle == null ? true : VE_SearchSeverityToggle.value;

            //With those values, generate and add the search filter objects
            searchFilters.Add(new OwnerFilter(searchOwner, searchUnowned));
            searchFilters.Add(new SeverityFilter(searchSeverity, searchHigherSeverity));

            //Apply filters
            ApplyFilterToTodoAttributes();
        }
        public void ApplyFilterToTodoAttributes()
        {
            if (searchFilters == null)
            {
                GenerateSearchFilters();
                if (searchFilters == null)
                    throw new NullReferenceException($"{nameof(TodoWindow)}: Search filters were still null " +
                        $"even after calling {nameof(GenerateSearchFilters)}() to regenerate them. " +
                        $"\nAvoiding infinite loop with by causing this Exception");
                
                return; //GenerateSearchFilters() calls this function too, so let that one proceed instead
            }
            if (todoAttributes_all == null)
            {
                GrabTodoAttributes();
                if (todoAttributes_all == null)
                    throw new NullReferenceException($"{nameof(TodoWindow)}: {nameof(todoAttributes_all)} was " +
                        $"still null even after calling {nameof(GrabTodoAttributes)}() to regenerate them. " +
                        $"\nAvoiding infinite loop with by causing this Exception");

                return; //GrabTodoAttributes() calls this function too, so let that one proceed instead
            }

            List<AttributeUsage> filtered = new List<AttributeUsage>();
            foreach (AttributeUsage todo in todoAttributes_all)
            {
                TodoAttribute todoAtt = todo.As<TodoAttribute>();
                bool passesFilter = true;

                foreach (SearchFilter filter in searchFilters)
                    passesFilter &= filter.IncludeThroughFilter(todoAtt);

                if (passesFilter)
                    filtered.Add(todo);
            }
            todoAttributes_filtered = filtered.ToArray();

            RefreshTodoListDisplay();
        }
        /// <summary>Generates a list of strings meant for the search options, with a string for each unique 
        /// <see cref="ToDoAttribute"/> owner including "All" as an option</summary>
        public void GenerateOwnerOptions()
        {
            if (todoAttributes_all == null)
            {
                GrabTodoAttributes();
                if (todoAttributes_all == null)
                    throw new NullReferenceException($"{nameof(TodoWindow)}: {nameof(todoAttributes_all)} was " +
                        $"still null even after calling {nameof(GrabTodoAttributes)}() to regenerate them. " +
                        $"\nAvoiding infinite loop with by causing this Exception");

                return; //GrabTodoAttributes() calls this function too, so let that one proceed instead
            }

            //New empty options list, starting with the "All" option with a divider
            ownerOptions = new List<string>() { "All", "Unowned", "" };
            foreach (AttributeUsage todo in todoAttributes_all)
            {
                string currentTodoOwner = todo.As<TodoAttribute>().Owner;
                if (!string.IsNullOrEmpty(currentTodoOwner)) //Only proceed if there is some kind of string
                {
                    //Drop names to lowercase for consistency
                    currentTodoOwner = currentTodoOwner.ToLower();
                    //Add to options if this is a unique owner
                    if (!ownerOptions.Contains(currentTodoOwner))
                        ownerOptions.Add(currentTodoOwner);
                }
            }
        }
        private void RefreshTodoListDisplay()
        {
            todoListDisplay = new TodoItemsDisplay(todoAttributes_filtered);
        }
        private void OnDrawTodoListDisplay()
        {
            if (todoListDisplay == null)
            {
                GrabTodoAttributes();
                if (todoListDisplay == null)
                    throw new Exception($"{nameof(TodoWindow)}: Grabbing {nameof(TodoAttribute)}s should have " +
                        $"eventually caused {nameof(todoListDisplay)} to get updated too, I am relying on this");
            }

            todoListDisplay.Draw(position);
        }

        public void ClearSearch()
        {
            //Directly set the value of each of these search option UI items, which will notify them of the change, and thus..
            //..EditorPrefs should automatically be updated too. Unfortunately "OnUpdatedSearch()" gets called 4 times, but oh well
            if (VE_SearchOwnerDropdown != null)    VE_SearchOwnerDropdown.value    = "All";
            if (VE_SearchOwnerToggle != null)      VE_SearchOwnerToggle.value      = true;
            if (VE_SearchSeverityDropdown != null) VE_SearchSeverityDropdown.value = TodoSeverity.Moderate;
            if (VE_SearchSeverityToggle != null)   VE_SearchSeverityToggle.value   = true;
        }

        //Called by Unity to set up rootVisualELement which contains all window information
        private void CreateGUI()
        {
            //Grab all the todoAttributes for this window (might not be necessary, and might happen again later, but it doesnt hurt)
            GrabTodoAttributes();

            //Attempt to clone the UI template for the rootVisualElement 
            if (RootVisualElementFromCloningUITemplate())
                //if successful, pull VisualElements from the cloned template and setup their functions
                SetupAllVisualElements();
            else
                //if not successful, fallback on old IMGUI display of ToDo items (There will be no search functionality)
                RootVisualElementFromNewIMGUIContainer();
        }

        #region VisualElement initialization setup and references

        /// <summary>Contains all visual information for the window. Cloned from <see cref="EUITemplate_TodoWindow"/>.
        /// <br/>Is used to pull certain <see cref="VisualElement"/>s from to set up the window's functionality.</summary>
        private VisualElement VE_RootWindowContainer;
        ///<summary>Clones <c>EUITemplate_TodoWindow</c> to the <c>rootVisualElement</c> and stores that clone 
        ///as <see cref="VE_RootWindowContainer"/></summary>
        private bool RootVisualElementFromCloningUITemplate()
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
                //Set the root visual element to be a clone of the UI template for the window provided
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
        private void RootVisualElementFromNewIMGUIContainer()
        {
            //set root visual element to be a new IMGUI container with the draw function for the todo list display
            rootVisualElement.Clear();
            VE_ListContainer = new IMGUIContainer(OnDrawTodoListDisplay);
            rootVisualElement.Add(VE_ListContainer);

            //Refresh the todo list display (might not be necessary, but doesnt hurt)
            RefreshTodoListDisplay();
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
                    $"Make sure you are calling {nameof(RootVisualElementFromCloningUITemplate)}() before calling " +
                    $"{nameof(SetupAllVisualElements)} to initialize {nameof(VE_RootWindowContainer)} (since that is " +
                    $"where all the {nameof(VisualElement)}s in this window pull their references from for setup).\n");

            //Call all setup functions for each visual element that we define functionality for
            Setup_VE_ListContainer();
            Setup_VE_SearchOwnerDropdown();
            Setup_VE_SearchOwnerToggle();
            Setup_VE_SearchSeverityDropdown();
            Setup_VE_SearchSeverityToggle();
            Setup_VE_SearchClearButton();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------- List Container ---------------------------------------------------------

        /// <summary>Container for displaying all of the TodoAttributes in a nice organized list with JumpTo buttons</summary>
        private IMGUIContainer VE_ListContainer;
        private const string ID_VE_LIST_CONTAINER = "List_Container";
        /// <summary>Sets up functionality of <see cref="VE_ListContainer"/></summary>
        private void Setup_VE_ListContainer()
        {
            //Get List Container from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_LIST_CONTAINER, out VE_ListContainer)) return;
            //setup gui of IMGUI container to call the draw function of the todo List
            VE_ListContainer.onGUIHandler = OnDrawTodoListDisplay;

            //Refresh the object for the todoListDisplay (might not be necessary, but doesnt hurt)
            RefreshTodoListDisplay();
        }

        //-----------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------- Owner Dropdown -----------------------------------------------------------

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
            if (ownerOptions == null) GenerateOwnerOptions();
            VE_SearchOwnerDropdown.choices = ownerOptions;

            //Setup to work with EditorPrefs to save settings per user
            string prefsSetting = EditorPrefs.GetString(
                PREFS_SEARCH_OWNER_DROPDOWN, VE_SearchOwnerDropdown.choices[0]); //Load from EditorPrefs
            if (!VE_SearchOwnerDropdown.choices.Contains(prefsSetting))
                prefsSetting = VE_SearchOwnerDropdown.choices[0];
            VE_SearchOwnerDropdown.SetValueWithoutNotify(prefsSetting); //Apply loaded value
            VE_SearchOwnerDropdown.RegisterValueChangedCallback(changeEvent => //On Value changed
            {
                EditorPrefs.SetString(PREFS_SEARCH_OWNER_DROPDOWN, changeEvent.newValue); //Save change to EditorPrefs
                OnUpdatedSearch(); //Update the search
            });
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //------------------------------------ Include Unowned Toggle -----------------------------------------------------------

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
            VE_SearchOwnerToggle.SetValueWithoutNotify(
                EditorPrefs.GetBool(PREFS_SEARCH_OWNER_TOGGLE, true)); //Load from EditorPrefs
            VE_SearchOwnerToggle.RegisterValueChangedCallback(changeEvent => //On Value changed
            {
                EditorPrefs.SetBool(PREFS_SEARCH_OWNER_TOGGLE, changeEvent.newValue); //Save change to EditorPrefs
                OnUpdatedSearch(); //Update the search
            });
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //--------------------------------------- Severity dropdown -------------------------------------------------------------

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
                EditorPrefs.GetInt(PREFS_SEARCH_SEVERITY_DROPDOWN, (int)TodoSeverity.Moderate)); //Load from EditorPrefs
            VE_SearchSeverityDropdown.RegisterValueChangedCallback(changeEvent => //On Value changed
            {
                EditorPrefs.SetInt(PREFS_SEARCH_SEVERITY_DROPDOWN, 
                    (int)(TodoSeverity)changeEvent.newValue); //Save change to EditorPrefs
                OnUpdatedSearch(); //Update the search
            });
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //----------------------------------- Include Higher Severity Toggle ----------------------------------------------------

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
                EditorPrefs.GetBool(PREFS_SEARCH_SEVERITY_TOGGLE, true)); //Load from EditorPrefs
            VE_SearchSeverityToggle.RegisterValueChangedCallback(changeEvent => //On Value changed
            {
                EditorPrefs.SetBool(PREFS_SEARCH_SEVERITY_TOGGLE, changeEvent.newValue);  //Save change to EditorPrefs
                OnUpdatedSearch(); //Update the search
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------- Clear Search Button -----------------------------------------------------

        //Button for clearing the search options
        private Button VE_SearchClearButton;
        private const string ID_VE_SEARCH_CLEAR_BUTTON = "Search_Clear_Button";
        /// <summary>Sets up functionality of <see cref="VE_SearchClearButton"/></summary>
        private void Setup_VE_SearchClearButton()
        {
            //Get "Search Severity Dropdown" from root, and escape function if it was not found
            if (!FindVisualElement(ID_VE_SEARCH_CLEAR_BUTTON, out VE_SearchClearButton)) return;

            VE_SearchClearButton.clicked += ClearSearch;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        #endregion







        /// <summary>This is an object that contains the information necessary, and logic to, create a DrawerObject
        /// to draw the entire TodoList.<br/>Also is responsible for grabbing the TodoAttributes and applying the
        /// given search filters</summary>
        private class TodoItemsDisplay
        {
            private NamespaceGroup noNamespaceGroup = new(null);
            private Dictionary<string, NamespaceGroup> namespaceGroups = new();
            private Vector2 scrollViewPos = Vector2.zero;
            public AttributeUsage[] AttributeUsages { get; private set; }
            public static float WindowWidth { get; private set; }

            public TodoItemsDisplay(AttributeUsage[] todoAttributes)
            {
                if (todoAttributes == null)
                    throw new NullReferenceException($"{nameof(TodoItemsDisplay)}: Passing null array of" +
                        $"attributes into constructor, but we need the attributes to draw the window");

                foreach (AttributeUsage todo in todoAttributes)
                    Register(todo.As<TodoAttribute>(), todo.Member);
            }

            public void Draw(Rect windowPosition)
            {
                WindowWidth = windowPosition.width;
                WindowWidth -= 16f;

                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
                EZ.DrawerObject itemsDisplayDrawerObject = null;

                //Try to get the drawerobject to draw for this todo list
                try { itemsDisplayDrawerObject = GetDrawerObject(); }
                catch (DivideByZeroException e)
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


            /// <summary>Respresents a subsection of the TodoList, </summary>
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
