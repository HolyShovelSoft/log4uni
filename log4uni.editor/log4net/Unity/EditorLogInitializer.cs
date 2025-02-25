using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Unity.Config;
using UnityEditor;

namespace log4net.Unity
{
    [InitializeOnLoad]
    public class EditorLogInitializer: AssetPostprocessor
    {
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

        private static List<Holder> _holders;

        static EditorLogInitializer()
        {
            Init();
        }

        private static void Init()
        {
            RuntimeLogInitializer.Init();

            var configurators = new PathConfigurator[]
            {
                AppDataPathConfigurator.Instance,
                AppPersistentDataPathConfigurator.Instance
            };
            
            _holders = new List<Holder>();

            for (var i = 0; i <= configurators.Length - 1; i++)
            {
                var configurator = configurators[i];
                var paths = configurator.FilePaths;
                for (var j = 0; j <= paths.Length - 1; j++)
                {
                    var path = paths[j];
                    _holders.Add(new Holder(configurator, path));
                }
            }
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            for (var i = 0; i <= _holders?.Count - 1; i++)
            {
                var holder = _holders[i];
                holder?.CheckChange();
            }
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