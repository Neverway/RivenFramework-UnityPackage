using System;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace RivenFramework
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class TodoAttribute : Attribute
    {
        //Attribute Options
        public string Owner = "";
        
        public readonly string Description;
        public readonly TodoSeverity Severity;
        public readonly string AttributeFileLocation;
        public readonly int AttributeLineLocation;

        public string RichTextDescription
        {
            get
            {
                switch (Severity)
                {
                    case TodoSeverity.Minor: return $"<color=#ffffff77>{Description}</color>";
                    case TodoSeverity.Major: return $"<color=#ffff88><b>{Description}</b></color>";
                    case TodoSeverity.Critical: return $"<color=#ff3333><b>{Description}</b></color>";
                }
                return Description;
            }
        }
        public TodoAttribute(
            string description = "- no description -", 
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
        {
            Description = description;
            Severity = severity;
            AttributeFileLocation = fileLocation;
            AttributeLineLocation = lineLocation;
        }

#if UNITY_EDITOR
        /// <summary>Opens your IDE at the file and line that this attribute was written</summary>
        public void EDITOR_OpenFileAtAttributeLocation() =>
            InternalEditorUtility.OpenFileAtLineExternal(AttributeFileLocation, AttributeLineLocation);
#endif
    }

    public enum TodoSeverity
    {
        Minor = 1 << 0,
        Moderate = 1 << 1,
        Major = 1 << 2,
        Critical = 1 << 3
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_AddCommentsAttribute : TodoAttribute
    {
        public Todo_AddCommentsAttribute(
            string details = "", 
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0) 
            : base($"Add comments and/or summaries{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_OptimizeAttribute : TodoAttribute
    {
        public Todo_OptimizeAttribute(
            string details = "",
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
            : base($"Optimize code{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_StressTestAttribute : TodoAttribute
    {
        public Todo_StressTestAttribute(
            string details = "",
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
            : base($"Stress test code{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_ImplementAttribute : TodoAttribute
    {
        public Todo_ImplementAttribute(
            string details = "",
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
            : base($"Finish implementation{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_PoorlyCodedAttribute : TodoAttribute
    {
        public Todo_PoorlyCodedAttribute(
            string details = "",
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
            : base($"Fix poor implementation{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class Todo_ToRemoveAttribute : TodoAttribute
    {
        public Todo_ToRemoveAttribute(
            string details = "",
            TodoSeverity severity = TodoSeverity.Moderate,
            [CallerFilePath] string fileLocation = "",
            [CallerLineNumber] int lineLocation = 0)
            : base($"Remove implementation{(details == "" ? "" : ": ")}{details}", 
                  severity, fileLocation, lineLocation) { }
    }
}
