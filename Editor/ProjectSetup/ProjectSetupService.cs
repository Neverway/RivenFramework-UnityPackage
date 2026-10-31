//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//


using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace RivenFramework.ProjectSetup
{
    [Serializable]
    public class GroupFile
    {
        public string displayName;
        public string description;
        public string destination;
        public string[] requires;
    }
 
    public class TemplateGroup
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public string Destination;
        public string[] Requires;
        public string SourcePath;
    }
 
    public enum EntryStatus
    {
        New,
        Exists,
        GuidConflict
    }
 
    public class PlanEntry
    {
        public string RelativePath;
        public string SourceFile;
        public string DestFile;
        public EntryStatus Status;
        public string Note;
    }
 
    public class ImportResult
    {
        public int Imported;
        public int SkippedExisting;
        public int SkippedConflict;
        public List<string> Messages = new List<string>();
    }
 
    public class HealthLine
    {
        public string Label;
        public bool Ok;
        public string Detail;
    }
 
    public static class ProjectSetupService
    {
        public const string PackageAssetPath = "Packages/com.neverway.riven";
        public const string TemplatesFolderName = "Templates~";
        public const string GroupFileName = "_group.json";
        public const string BuildScenesFileName = "_build-scenes.txt";
        public const string FoldersFileName = "_folders.txt";
 
        private static readonly string[] DefaultFolders =
        {
            "Resources/Maps",
            "Resources/Objects",
            "Resources/Other",
            "Resources/Scripts",
            "Resources/Widgets",
            "Resources/Models",
            "Resources/Models/Materials",
            "Resources/Models/Animations",
            "Resources/Sound/Music",
            "Resources/Sound/Effects",
            "Resources/Materials/Textures",
            "Resources/Materials/Shaders",
            "Resources/Materials/Skyboxes",
            "Resources/Materials/Effects",
            "Resources/Materials/PhysicsMaterials",
            "Resources/Sprites/Level",
            "Resources/Sprites/Widgets",
            "Resources/Sprites/Tilesets",
            "Resources/Sprites/Items",
            "Resources/Sprites/Effects",
            "Resources/Sprites/Characters"
        };
 
        public static string ProjectRoot
        {
            get { return Directory.GetParent(Application.dataPath).FullName; }
        }
 
        public static PackageInfo GetPackageInfo()
        {
            PackageInfo info = PackageInfo.FindForAssembly(typeof(ProjectSetupService).Assembly);
            if (info == null) info = PackageInfo.FindForAssetPath(PackageAssetPath + "/package.json");
            return info;
        }
 
        public static string GetTemplatesRoot()
        {
            PackageInfo info = GetPackageInfo();
            if (info == null || string.IsNullOrEmpty(info.resolvedPath)) return null;
            string path = Path.Combine(info.resolvedPath, TemplatesFolderName);
            return Directory.Exists(path) ? path : null;
        }
 
        private static string NormalizeRel(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            return path.Replace('\\', '/').Trim('/');
        }
 
        private static string RelativeTo(string root, string fullPath)
        {
            string rel = fullPath.Substring(root.Length).Replace('\\', '/');
            return rel.TrimStart('/');
        }
 
        private static string ReadGuid(string metaPath)
        {
            if (!File.Exists(metaPath)) return null;
            foreach (string line in File.ReadLines(metaPath))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("guid:")) return trimmed.Substring(5).Trim();
            }
            return null;
        }
 
        public static List<string> GetStandardFolders()
        {
            string root = GetTemplatesRoot();
            if (root != null)
            {
                string file = Path.Combine(root, FoldersFileName);
                if (File.Exists(file))
                {
                    List<string> custom = new List<string>();
                    foreach (string raw in File.ReadAllLines(file))
                    {
                        string line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        custom.Add(NormalizeRel(line));
                    }
                    if (custom.Count > 0) return custom;
                }
            }
            return new List<string>(DefaultFolders);
        }
 
        public static List<string> GetMissingFolders()
        {
            List<string> missing = new List<string>();
            foreach (string folder in GetStandardFolders())
            {
                if (!Directory.Exists(Path.Combine(Application.dataPath, folder))) missing.Add(folder);
            }
            return missing;
        }
 
        public static int CreateStandardFolders()
        {
            List<string> folders = GetStandardFolders();
            int created = 0;
 
            foreach (string folder in folders)
            {
                string abs = Path.Combine(Application.dataPath, folder);
                if (!Directory.Exists(abs))
                {
                    Directory.CreateDirectory(abs);
                    created++;
                }
            }
 
            foreach (string folder in folders)
            {
                bool isLeaf = true;
                foreach (string other in folders)
                {
                    if (other != folder && other.StartsWith(folder + "/")) { isLeaf = false; break; }
                }
                if (!isLeaf) continue;
 
                string abs = Path.Combine(Application.dataPath, folder);
                if (Directory.GetFiles(abs).Length == 0 && Directory.GetDirectories(abs).Length == 0)
                {
                    File.WriteAllText(Path.Combine(abs, ".gitkeep"), "");
                }
            }
 
            AssetDatabase.Refresh();
            return created;
        }
 
        public static List<TemplateGroup> LoadGroups()
        {
            List<TemplateGroup> result = new List<TemplateGroup>();
            string root = GetTemplatesRoot();
            if (root == null) return result;
 
            foreach (string dir in Directory.GetDirectories(root))
            {
                string id = Path.GetFileName(dir);
                TemplateGroup group = new TemplateGroup
                {
                    Id = id,
                    DisplayName = id,
                    Description = "",
                    Destination = "",
                    Requires = new string[0],
                    SourcePath = dir
                };
 
                string jsonPath = Path.Combine(dir, GroupFileName);
                if (File.Exists(jsonPath))
                {
                    try
                    {
                        GroupFile gf = JsonUtility.FromJson<GroupFile>(File.ReadAllText(jsonPath));
                        if (gf != null)
                        {
                            if (!string.IsNullOrEmpty(gf.displayName)) group.DisplayName = gf.displayName;
                            group.Description = gf.description ?? "";
                            group.Destination = NormalizeRel(gf.destination);
                            group.Requires = gf.requires ?? new string[0];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[Riven Framework Setup] Could not read " + jsonPath + ": " + e.Message);
                    }
                }
 
                result.Add(group);
            }
 
            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));
            return result;
        }
 
        public static List<PlanEntry> BuildPlan(TemplateGroup group, string destination)
        {
            List<PlanEntry> plan = new List<PlanEntry>();
            string destRoot = Path.Combine(Application.dataPath, NormalizeRel(destination));
 
            foreach (string file in Directory.GetFiles(group.SourcePath, "*", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(file);
                if (name.EndsWith(".meta") || name == GroupFileName || name == ".DS_Store" || name == "Thumbs.db") continue;
 
                string rel = RelativeTo(group.SourcePath, file);
                PlanEntry entry = new PlanEntry
                {
                    RelativePath = rel,
                    SourceFile = file,
                    DestFile = Path.Combine(destRoot, rel),
                    Status = EntryStatus.New,
                    Note = ""
                };
 
                if (File.Exists(entry.DestFile))
                {
                    entry.Status = EntryStatus.Exists;
                }
                else
                {
                    string guid = ReadGuid(file + ".meta");
                    if (guid != null)
                    {
                        string existing = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrEmpty(existing))
                        {
                            entry.Status = EntryStatus.GuidConflict;
                            entry.Note = existing;
                        }
                    }
                }
 
                plan.Add(entry);
            }
 
            plan.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
            return plan;
        }
 
        public static ImportResult Import(IEnumerable<TemplateGroup> groups, Func<TemplateGroup, string> destinationFor)
        {
            ImportResult result = new ImportResult();
 
            foreach (TemplateGroup group in groups)
            {
                string destination = NormalizeRel(destinationFor(group));
                string destRoot = Path.Combine(Application.dataPath, destination);
                List<PlanEntry> plan = BuildPlan(group, destination);
 
                foreach (PlanEntry entry in plan)
                {
                    if (entry.Status == EntryStatus.Exists)
                    {
                        result.SkippedExisting++;
                        continue;
                    }
 
                    if (entry.Status == EntryStatus.GuidConflict)
                    {
                        result.SkippedConflict++;
                        result.Messages.Add("Skipped " + entry.RelativePath + " (its GUID is already used by " + entry.Note + ")");
                        continue;
                    }
 
                    try
                    {
                        string relDir = Path.GetDirectoryName(entry.RelativePath);
                        EnsureDirectories(group.SourcePath, destRoot, relDir);
 
                        File.Copy(entry.SourceFile, entry.DestFile, false);
                        string srcMeta = entry.SourceFile + ".meta";
                        if (File.Exists(srcMeta)) File.Copy(srcMeta, entry.DestFile + ".meta", true);
                        result.Imported++;
                    }
                    catch (Exception e)
                    {
                        result.Messages.Add("Failed to copy " + entry.RelativePath + ": " + e.Message);
                    }
                }
            }
 
            AssetDatabase.Refresh();
            return result;
        }
 
        private static void EnsureDirectories(string srcRoot, string destRoot, string relDir)
        {
            if (string.IsNullOrEmpty(relDir))
            {
                Directory.CreateDirectory(destRoot);
                return;
            }
 
            string cumulative = "";
            string[] parts = relDir.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                cumulative = cumulative.Length == 0 ? part : cumulative + "/" + part;
                string destDir = Path.Combine(destRoot, cumulative);
                if (Directory.Exists(destDir)) continue;
 
                Directory.CreateDirectory(destDir);
 
                string srcMeta = Path.Combine(srcRoot, cumulative) + ".meta";
                string destMeta = destDir + ".meta";
                if (File.Exists(srcMeta) && !File.Exists(destMeta))
                {
                    string guid = ReadGuid(srcMeta);
                    bool conflict = guid != null && !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid));
                    if (!conflict) File.Copy(srcMeta, destMeta);
                }
            }
        }
 
        public static string RegisterBuildScenes()
        {
            string root = GetTemplatesRoot();
            if (root == null) return "No Templates~ folder was found in the package.";
 
            string file = Path.Combine(root, BuildScenesFileName);
            if (!File.Exists(file)) return "No " + BuildScenesFileName + " file was found in Templates~.";
 
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int added = 0;
            int alreadyThere = 0;
            List<string> missing = new List<string>();
 
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim().Replace('\\', '/');
                if (line.Length == 0 || line.StartsWith("#")) continue;
 
                if (!File.Exists(Path.Combine(ProjectRoot, line)))
                {
                    missing.Add(line);
                    continue;
                }
 
                string guid = AssetDatabase.AssetPathToGUID(line);
                bool present = false;
                foreach (EditorBuildSettingsScene s in scenes)
                {
                    if (s.path == line || (!string.IsNullOrEmpty(guid) && s.guid.ToString() == guid))
                    {
                        present = true;
                        break;
                    }
                }
 
                if (present)
                {
                    alreadyThere++;
                    continue;
                }
 
                scenes.Add(new EditorBuildSettingsScene(line, true));
                added++;
            }
 
            EditorBuildSettings.scenes = scenes.ToArray();
 
            string message = "Build Settings: added " + added + ", already present " + alreadyThere + ".";
            if (missing.Count > 0)
            {
                message += " Not found in project (import the templates first?): " + string.Join(", ", missing.ToArray());
            }
            return message;
        }
 
        //=-----------------=
        // Health check
        //=-----------------=
        public static List<HealthLine> RunHealthCheck()
        {
            List<HealthLine> lines = new List<HealthLine>();
 
            PackageInfo info = GetPackageInfo();
            lines.Add(new HealthLine
            {
                Label = "Riven Framework package",
                Ok = info != null,
                Detail = info != null ? "version " + info.version : "package not found"
            });
 
            string unity = Application.unityVersion;
            lines.Add(new HealthLine
            {
                Label = "Unity version",
                Ok = unity.StartsWith("2022.3"),
                Detail = unity + (unity.StartsWith("2022.3") ? "" : " (framework targets 2022.3)")
            });
 
            HashSet<string> packages = new HashSet<string>();
            try
            {
                foreach (PackageInfo p in PackageInfo.GetAllRegisteredPackages()) packages.Add(p.name);
            }
            catch (Exception)
            {
            }
 
            string[] required = { "com.unity.inputsystem", "com.unity.textmeshpro", "com.unity.ugui" };
            foreach (string name in required)
            {
                bool has = packages.Contains(name);
                lines.Add(new HealthLine { Label = name, Ok = has, Detail = has ? "installed" : "missing" });
            }
 
            bool markup = false;
            try
            {
                string manifest = Path.Combine(ProjectRoot, "Packages/manifest.json");
                markup = File.Exists(manifest) && File.ReadAllText(manifest).Contains("com.gasgiant.markupattributes");
            }
            catch (Exception)
            {
            }
            lines.Add(new HealthLine
            {
                Label = "com.gasgiant.markupattributes",
                Ok = markup,
                Detail = markup ? "in manifest.json" : "missing from Packages/manifest.json (see the package README)"
            });
 
            return lines;
        }
    }

}
