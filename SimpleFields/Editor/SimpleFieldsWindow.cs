using EasyInspector;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static EasyPropertyDrawer;

    public class SimpleFieldsWindow : EditorWindow
    {
        public static SimpleFieldsWindow currentWindow;
        public SimpleFields target;
        public EasyProperty property;
        public SerializedObject[] allSerializedObjects;
        public VerticalGroup contents;
        public bool editName = false;
        public string editedNameText;
        public bool refreshContent = false;
        public Vector2 scrollPos;
        public Rect previousSize;

        public static void DisplayPopup(SimpleFields target)
        {
            if (currentWindow != null)
            {
                if (!currentWindow.docked)
                    currentWindow.Close();
            }
            currentWindow = CreateInstance<SimpleFieldsWindow>();
            currentWindow.target = target;
            currentWindow.SetProperties();
            currentWindow.RefreshContent();

            Rect area = new Rect(HandleUtility.WorldToGUIPoint(target.transform.position), currentWindow.StartingWindowSize);
            area.position -= area.size * 0.5f;
            currentWindow.position = area;
            currentWindow.Show();
        }

        public Vector2 StartingWindowSize => new Vector2(500, contents.GetHeight());

        public void Update()
        {
            if (target == null)
            {
                Close();
                return;
            }

            if (docked)
                return;

            if (Selection.activeGameObject != target.gameObject)
            {
                Close();
                return;
            }
        }

        public void OnGUI()
        {
            if (refreshContent || contents == null)
                RefreshContent();

            foreach (SerializedObject obj in allSerializedObjects)
                obj.Update();

            contents.Draw(new Rect(Vector2.zero, position.size));
            titleContent = new GUIContent(target.name);

            //Refresh content if window was resized
            if (previousSize != position)
                refreshContent = true;

            previousSize = position;
        }

        public void RefreshContent()
        {
            if (property == null)
                SetProperties();

            contents = new VerticalGroup(FieldContainerDrawer.GetSimplifiedFields(property));
            contents = new VerticalGroup(GetPopupHeader()).Add(ToScrollView(contents));
        }

        public void SetProperties()
        {
            property = new EasyProperty(target, nameof(SimpleFields.fc));

            allSerializedObjects = property["categories"]
                .SelectMany(category => category["fields"])
                .Where(fieldRef => fieldRef["targetComponent"].AsUnityObject != null)
                .Select(fieldRef => new SerializedObject(fieldRef["targetComponent"].AsUnityObject))
                .ToArray();
        }

        public DrawerObject GetPopupHeader()
        {
            VerticalGroup header = new VerticalGroup();
            DrawerObject objectNameLabel;
            if (editName)
            {
                objectNameLabel = new TextField(editedNameText, text => editedNameText = text).OnConfirm("SimpFieldNameEdit", ConfirmEditedName);
            }
            else
                objectNameLabel = new Label(target.name).Bold().AlignCenter().Big2();

            header.Add(new SizedHorizontalGroup(objectNameLabel)
                .AddOnRight(new Button("Close", Close).Small(), 70)
                .AddOnLeft(GetEditNameButton(), 70));
            header.Add(new Divider());
            header.Add(new EmptySpace(5));

            Boxed box = new Boxed(header, 7); //padding
            box.boldness = 0;

            return box;
        }

        public DrawerObject ToScrollView(DrawerObject drawerObj)
        {
            Boxed box = new Boxed(drawerObj, 7); //padding
            box.boldness = 0;
            return new ScrollGroup(box, position.size.y - GetPopupHeader().GetHeight(), () => scrollPos, pos => scrollPos = pos);
        }
        public DrawerObject GetEditNameButton()
        {
            if (editName)
                return new Button("Confirm", ConfirmEditedName).Small();
            return new Button("Edit Name", StartEditingName).Small();
        }
        public void StartEditingName()
        {
            editedNameText = target.name;

            editName = true;
            refreshContent = true;
            Repaint();
        }

        public void ConfirmEditedName()
        {
            if (target.name != editedNameText)
            {
                Undo.RecordObject(target.gameObject, "Renamed " + target.name + " to " + editedNameText);
                target.name = editedNameText;
                EditorUtility.SetDirty(target.gameObject);
            }

            editName = false;
            refreshContent = true;
            Repaint();
        }
        public void ConfirmEditedName(string newName)
        {
            editedNameText = newName;
            ConfirmEditedName();
        }
    }
