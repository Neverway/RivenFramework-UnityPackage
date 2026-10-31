//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RivenFramework.ProjectSetup
{
    public class RivenProjectSetupWindow : EditorWindow
    {
        private Vector2 scroll;
        private List<TemplateGroup> groups = new List<TemplateGroup>();
        private readonly HashSet<string> selected = new HashSet<string>();
        private readonly Dictionary<string, string> destinations = new Dictionary<string, string>();
        private readonly Dictionary<string, List<PlanEntry>> plans = new Dictionary<string, List<PlanEntry>>();
        private readonly Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
        private List<string> missingFolders = new List<string>();
        private List<HealthLine> health = new List<HealthLine>();
        private bool showMissingFolders;
        private string message = "";
        private MessageType messageType = MessageType.None;

        [MenuItem("Neverway/Riven Framework/Project Setup")]
        public static void Open()
        {
            RivenProjectSetupWindow window = GetWindow<RivenProjectSetupWindow>("Riven Setup");
            window.minSize = new Vector2(460, 520);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            groups = ProjectSetupService.LoadGroups();

            HashSet<string> ids = new HashSet<string>();
            foreach (TemplateGroup g in groups) ids.Add(g.Id);
            selected.RemoveWhere(id => !ids.Contains(id));

            foreach (TemplateGroup g in groups)
            {
                if (!destinations.ContainsKey(g.Id)) destinations[g.Id] = g.Destination;
                plans[g.Id] = ProjectSetupService.BuildPlan(g, destinations[g.Id]);
            }

            missingFolders = ProjectSetupService.GetMissingFolders();
            health = ProjectSetupService.RunHealthCheck();
            Repaint();
        }

        private TemplateGroup FindGroup(string id)
        {
            foreach (TemplateGroup g in groups)
            {
                if (g.Id == id) return g;
            }
            return null;
        }

        private HashSet<string> ComputeRequired()
        {
            HashSet<string> required = new HashSet<string>();
            Queue<string> queue = new Queue<string>(selected);
            while (queue.Count > 0)
            {
                TemplateGroup g = FindGroup(queue.Dequeue());
                if (g == null) continue;
                foreach (string r in g.Requires)
                {
                    if (FindGroup(r) != null && required.Add(r)) queue.Enqueue(r);
                }
            }
            return required;
        }

        private void SetMessage(string text, MessageType type)
        {
            message = text;
            messageType = type;
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawHeader();
            EditorGUILayout.Space(8);
            DrawFolderSection();
            EditorGUILayout.Space(8);
            DrawTemplateSection();
            EditorGUILayout.Space(8);
            DrawBuildSettingsSection();
            EditorGUILayout.Space(8);
            DrawHealthSection();

            if (!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(message, messageType);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Riven Framework Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Setup the project folder structure and import the Frameworks template assets.",
                EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Refresh"))
            {
                RefreshAll();
                SetMessage("", MessageType.None);
            }
        }

        private void DrawFolderSection()
        {
            EditorGUILayout.LabelField("Folder Structure", EditorStyles.boldLabel);

            if (missingFolders.Count == 0)
            {
                EditorGUILayout.HelpBox("OK", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField(missingFolders.Count + " standard folder(s) are missing.");

            showMissingFolders = EditorGUILayout.Foldout(showMissingFolders, "Show missing folders");
            if (showMissingFolders)
            {
                EditorGUI.indentLevel++;
                foreach (string folder in missingFolders) EditorGUILayout.LabelField("Assets/" + folder);
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Create standard folders", GUILayout.Height(24)))
            {
                int created = ProjectSetupService.CreateStandardFolders();
                RefreshAll();
                SetMessage("Created " + created + " folder(s).", MessageType.Info);
            }
        }

        private void DrawTemplateSection()
        {
            EditorGUILayout.LabelField("Template assets", EditorStyles.boldLabel);

            if (groups.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No folder templates could be found. The packages should contain a Templates~ folder with the template folder structure within it.",
                    MessageType.Warning);
                return;
            }

            HashSet<string> required = ComputeRequired();

            foreach (TemplateGroup group in groups)
            {
                bool isRequired = required.Contains(group.Id);
                bool isOn = isRequired || selected.Contains(group.Id);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                using (new EditorGUI.DisabledScope(isRequired))
                {
                    string label = group.DisplayName + (isRequired ? "  (required by another selection)" : "");
                    bool newValue = EditorGUILayout.ToggleLeft(label, isOn);
                    if (!isRequired)
                    {
                        if (newValue) selected.Add(group.Id);
                        else selected.Remove(group.Id);
                    }
                }

                if (!string.IsNullOrEmpty(group.Description))
                {
                    EditorGUILayout.LabelField(group.Description, EditorStyles.wordWrappedMiniLabel);
                }

                EditorGUI.BeginChangeCheck();
                string dest = EditorGUILayout.TextField("Copy into Assets/", destinations[group.Id]);
                if (EditorGUI.EndChangeCheck())
                {
                    destinations[group.Id] = dest;
                    plans[group.Id] = ProjectSetupService.BuildPlan(group, dest);
                }

                List<PlanEntry> plan = plans[group.Id];
                int fresh = 0, exists = 0, conflict = 0;
                foreach (PlanEntry e in plan)
                {
                    if (e.Status == EntryStatus.New) fresh++;
                    else if (e.Status == EntryStatus.Exists) exists++;
                    else conflict++;
                }
                EditorGUILayout.LabelField(
                    fresh + " new, " + exists + " already exist (skipped), " + conflict + " GUID conflict (skipped)",
                    EditorStyles.miniLabel);

                bool open;
                foldouts.TryGetValue(group.Id, out open);
                open = EditorGUILayout.Foldout(open, "Files");
                foldouts[group.Id] = open;
                if (open)
                {
                    EditorGUI.indentLevel++;
                    int shown = 0;
                    foreach (PlanEntry e in plan)
                    {
                        if (shown >= 200)
                        {
                            EditorGUILayout.LabelField("... and " + (plan.Count - shown) + " more");
                            break;
                        }

                        string tag = e.Status == EntryStatus.New ? "[new]  "
                            : e.Status == EntryStatus.Exists ? "[exists]  "
                            : "[GUID in use by " + e.Note + "]  ";
                        EditorGUILayout.LabelField(tag + e.RelativePath, EditorStyles.miniLabel);
                        shown++;
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            using (new EditorGUI.DisabledScope(selected.Count == 0))
            {
                if (GUILayout.Button("Import selected templates", GUILayout.Height(24)))
                {
                    ImportSelected(required);
                }
            }
        }

        private void ImportSelected(HashSet<string> required)
        {
            List<TemplateGroup> toImport = new List<TemplateGroup>();
            foreach (TemplateGroup g in groups)
            {
                if (selected.Contains(g.Id) || required.Contains(g.Id)) toImport.Add(g);
            }

            int fresh = 0;
            foreach (TemplateGroup g in toImport)
            {
                foreach (PlanEntry e in plans[g.Id])
                {
                    if (e.Status == EntryStatus.New) fresh++;
                }
            }

            if (fresh == 0)
            {
                SetMessage("Nothing to import: every file in the selected groups already exists or has a GUID conflict.", MessageType.Info);
                return;
            }

            bool go = EditorUtility.DisplayDialog(
                "Import templates",
                "This will copy " + fresh + " new file(s) into your project from " + toImport.Count +
                " group(s). Existing files are not touched.\n\nTip: Please commit or backup your project first!",
                "Import",
                "Cancel");
            if (!go) return;

            ImportResult result = ProjectSetupService.Import(toImport, g => destinations[g.Id]);
            RefreshAll();

            string text = "Imported " + result.Imported + " file(s). Skipped " + result.SkippedExisting +
                          " existing and " + result.SkippedConflict + " with GUID conflicts.";
            foreach (string m in result.Messages) text += "\n" + m;
            SetMessage(text, result.Messages.Count > 0 ? MessageType.Warning : MessageType.Info);
        }

        private void DrawBuildSettingsSection()
        {
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Add the framework scenes listed in Templates~/_build-scenes.txt to the current project Build Settings",
                EditorStyles.wordWrappedMiniLabel);

            if (GUILayout.Button("Register framework scenes", GUILayout.Height(24)))
            {
                string result = ProjectSetupService.RegisterBuildScenes();
                SetMessage(result, MessageType.Info);
            }
        }

        private void DrawHealthSection()
        {
            EditorGUILayout.LabelField("Project health", EditorStyles.boldLabel);
            foreach (HealthLine line in health)
            {
                string mark = line.Ok ? "OK    " : "CHECK ";
                EditorGUILayout.LabelField(mark + line.Label + " - " + line.Detail, EditorStyles.miniLabel);
            }
        }
    }
}