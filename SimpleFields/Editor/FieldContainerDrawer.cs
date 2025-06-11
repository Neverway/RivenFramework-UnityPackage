using UnityEditor;
using UnityEngine;
using EasyInspector;

    [CustomPropertyDrawer(typeof(SimpleFields.FieldContainer))]
    internal class FieldContainerDrawer : EasyPropertyDrawer
    {
        bool pickFieldsMode = false;
        static DrawerObject space = new EmptySpace(5);
        EasyProperty mainProperty;
        public override DrawerObject OnGUIEasyDrawer(VerticalGroup contents)
        {
            DrawerObject pickFieldsModeButton =
                new Button(pickFieldsMode ? "Back to Editing Fields" : "Pick Fields", TogglePickFieldsMode).AlignCenter();

            mainProperty = new EasyProperty(property.Property);

            if (pickFieldsMode)
            {
                contents.Add(new Property(mainProperty["categories"]).IncludeChildren());
            }
            else
                contents.Add(GetSimplifiedFields(mainProperty));

            contents.Add(new Divider());
            contents.Add(new HorizontalGroup().Add(space).Add(space).Add(pickFieldsModeButton));

            return contents;
        }

        public static DrawerObject GetSimplifiedFields(EasyProperty mainProperty)
        {
            VerticalGroup contents = new VerticalGroup();
            foreach (EasyProperty categoryProp in mainProperty["categories"])
            {
                if (categoryProp["fields"].Count == 0)
                    continue;

                string title = categoryProp["categoryName"].AsString;
                if (!string.IsNullOrEmpty(title))
                {
                    Label categoryHeader = new Label(title);
                    categoryHeader.Bold().style.fontSize = 16;
                    contents.Add(categoryHeader);
                }

                VerticalGroup categoryContent = new VerticalGroup();

                foreach (EasyProperty fieldProp in categoryProp["fields"])
                {
                    Component selectedComponent = fieldProp["targetComponent"].Get<Component>();

                    if (selectedComponent == null)
                    {
                        categoryContent.Add(GetErrorLabel("Component Not Found"));
                        continue;
                    }

                    string selectedFieldName = fieldProp["fieldName"].AsString;

                    if (string.IsNullOrEmpty(selectedFieldName))
                    {
                        categoryContent.Add(GetErrorLabel("No Field Selected"));
                        continue;
                    }

                    EasyProperty selectedProperty = new EasyProperty(selectedComponent, selectedFieldName);

                    if (selectedProperty.Property == null)
                    {
                        categoryContent.Add(GetErrorLabel("Invalid Field Selected"));
                        continue;
                    }

                    categoryContent.Add(new Property(selectedProperty)
                        .IncludeChildren()
                        .UpdateSerializedObject()
                        );
                }
                contents.Add(new Boxed(categoryContent));
                contents.Add(space);
            }
            return contents;
        }

        public static DrawerObject GetErrorLabel(string errorText)
        {
            errorText = "<b><i><color=#dd3333ff>Error: " + errorText + "</color></i></b>";
            Divider line = new Divider().Color(new Color(0.866f, 0.2f, 0.2f, 0.5f));
            FittedLabel warningLabel = new FittedLabel(errorText, line).AndBeforeLabel(line);
            warningLabel.UseRichText().AlignCenter();
            return warningLabel;
        }

        public override void OnAfterGUI()
        {
            Component self = mainProperty.SerializedObject.targetObject as Component;

            foreach (EasyProperty category in mainProperty["categories"])
            {
                foreach (EasyProperty fieldRef in category["fields"])
                {
                    GameObject gameObject = fieldRef["targetGameObject"].AsUnityObject as GameObject;
                    if (gameObject == null)
                        continue;

                    if (!gameObject.transform.IsChildOf(self.transform))
                    {
                        fieldRef["targetGameObject"].AsUnityObject = null;
                        fieldRef["targetComponent"].AsUnityObject = null;
                        fieldRef["fieldName"].AsString = null;
                    }

                    if (fieldRef["targetComponent"].AsUnityObject is SimpleFields)
                    {
                        fieldRef["targetComponent"].AsUnityObject = null;
                        fieldRef["fieldName"].AsString = null;
                    }

                    if (fieldRef["targetComponent"].AsUnityObject is Transform)
                    {
                        fieldRef["targetComponent"].AsUnityObject = null;
                        fieldRef["fieldName"].AsString = null;
                    }
                }
            }
        }

        public void TogglePickFieldsMode() => pickFieldsMode = !pickFieldsMode;
    }