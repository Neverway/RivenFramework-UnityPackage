using EasyInspector;
using UnityEditor;
using UnityEngine;

    [CustomPropertyDrawer(typeof(SimpleFields.FieldReference))]
    internal class FieldReferenceDrawer : EasyPropertyDrawer
    {
        public string targetGameObject = nameof(SimpleFields.FieldReference.targetGameObject);
        public string targetComponent = nameof(SimpleFields.FieldReference.targetComponent);
        public string fieldName = nameof(SimpleFields.FieldReference.fieldName);

        public Component oldComponent;
        public override DrawerObject OnGUIEasyDrawer(VerticalGroup contents)
        {
            HorizontalGroup line = new HorizontalGroup();

            if (HasComponent)
            {
                SetGameObjectAsComponentsGameObject();
                GUISelectField(line);
            }
            else
            {
                if (HasGameObject)
                    GUISelectComponentFromGameObject(line);
                else
                    GUISelectGameObjectOrComponent(line);
            }

            contents.Add(line);

            return contents;

        }

        public override void OnAfterGUI()
        {
            if (!HasComponent || oldComponent == null)
                return;

            Component newComponent = GetComponent;
            if ((oldComponent != newComponent) && HasGameObject && (newComponent is Transform))
            {
                property[targetComponent].Property.objectReferenceValue = null;
                property[targetGameObject].Property.objectReferenceValue = oldComponent.gameObject;
            }
        }

        public void GUISelectGameObjectOrComponent(HorizontalGroup line)
        {
            //line.Add(new Property(property[targetGameObject]).HideLabel());
            line.Add(new Property(property[targetComponent]).HideLabel());
        }
        public void GUISelectComponentFromGameObject(HorizontalGroup line)
        {
            line.Add(new Property(property[targetGameObject]).HideLabel());
            line.Add(new SelectComponentFromGameObject(
                (GameObject)property[targetGameObject].Property.objectReferenceValue,
                property[targetComponent])
                );
        }
        public void GUISelectField(HorizontalGroup line)
        {
            line.Add(new Property(property[targetComponent]).HideLabel());
            line.Add(new SelectFieldDropdown(
                property[targetComponent].Property.objectReferenceValue,
                property[fieldName])
                );
        }

        public void SetGameObjectAsComponentsGameObject()
        {
            property[targetGameObject].Property.objectReferenceValue =
                ((Component)property[targetComponent].Property.objectReferenceValue).gameObject;
        }

        public bool HasComponent => property[targetComponent].Property.objectReferenceValue != null;
        public bool HasGameObject => property[targetGameObject].Property.objectReferenceValue != null;

        public GameObject GetGameObject => (GameObject)property[targetGameObject].Property.objectReferenceValue;
        public Component GetComponent => (Component)property[targetComponent].Property.objectReferenceValue;

    }