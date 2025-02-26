using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using log4net.Unity.Config;
using UnityEditor;

namespace log4net.Unity
{
    public class EditorLogInitializer: AssetPostprocessor
    {
        private const float SECONDS_BETWEEN_CHECKS = 1;

        private class Holder
        {
            private bool lastExist;
            private DateTime lastEdit;
            private readonly FileInfo fileInfo;
            private readonly PathConfigurator configurator;

            public void CheckChange()
            {
                if (fileInfo == null) return;
                fileInfo.Refresh();
                if (fileInfo.Exists == lastExist && fileInfo.LastWriteTime == lastEdit) return;
                lastExist = fileInfo.Exists;
                lastEdit = fileInfo.LastWriteTime;
                configurator?.CallChange();
            }

            public Holder(PathConfigurator configurator, string path)
            {
                if(string.IsNullOrEmpty(path)) return;
                path = Path.GetFullPath(path);
                fileInfo = new FileInfo(path);
                lastExist = fileInfo.Exists;
                lastEdit = fileInfo.LastWriteTime;
                this.configurator = configurator;
            }
        }

        private static List<Holder> Holders;
        private static bool Initialized;
        private static Stopwatch Counter;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (Initialized) return;
            Initialized = true;

            Counter = Stopwatch.StartNew();

            var configurators = new PathConfigurator[]
            {
                AppDataPathConfigurator.Instance,
                AppPersistentDataPathConfigurator.Instance
            };
            
            Holders = new List<Holder>();

            for (var i = 0; i <= configurators.Length - 1; i++)
            {
                var configurator = configurators[i];
                var paths = configurator.FilePaths;
                for (var j = 0; j <= paths.Length - 1; j++)
                {
                    var path = paths[j];
                    Holders.Add(new Holder(configurator, path));
                }
            }
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (Counter.Elapsed.TotalSeconds < SECONDS_BETWEEN_CHECKS)
            {
                return;
            }

            for (var i = 0; i <= Holders?.Count - 1; i++)
            {
                var holder = Holders[i];
                holder?.CheckChange();
            }

            Counter.Restart();
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var exists = (importedAssets ?? [])
                .Concat(deletedAssets ?? [])
                .Concat(movedAssets ?? [])
                .Concat(movedFromAssetPaths ?? [])
                .Distinct()
                .Where(s =>
                {
                    if (string.IsNullOrEmpty(s)) return false;
                    var directories = Path.GetDirectoryName(s)?
                        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .ToList() ?? new List<string>();
                    var name = Path.GetFileNameWithoutExtension(s);
                    return directories.Any(dir => dir?.ToLower() == "resources") && name.ToLower() == "log4net";
                }).Any();

            if (exists)
            {
                ResourceConfigurator.Instance.CallChange();
            }
        }
    }
}