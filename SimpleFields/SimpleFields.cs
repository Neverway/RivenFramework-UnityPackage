using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent, AddComponentMenu("SimpleFields")]
    public class SimpleFields : MonoBehaviour
    {
        #if UNITY_EDITOR
        //FieldContainer is just to make all the fields here into a single field itself
        //so that I can use the EasyDrawer class I made. This is bad, change this later
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] internal FieldContainer fc;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (!Application.isPlaying)
            {
                MoveComponentToTop(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        public static void MoveComponentToTop(SimpleFields component)
        { 
            // Use UnityEditorInternal to move up the component repeatedly
            var moveUpMethod = typeof(UnityEditorInternal.ComponentUtility)
                .GetMethod("MoveComponentUp", BindingFlags.Static | BindingFlags.Public);

            while (moveUpMethod != null && moveUpMethod.Invoke(null, new object[] { component }) is true) { }
        }


        [System.Serializable]
        internal struct FieldContainer
        {
            [SerializeField] public Category[] categories;
        }

        [System.Serializable]
        internal struct Category
        {
            public string categoryName;
            [SerializeField] public FieldReference[] fields;
        }

        [System.Serializable]
        internal struct FieldReference
        {
            public GameObject targetGameObject;
            public Component targetComponent;
            public string fieldName;
        }
        #endif
    }