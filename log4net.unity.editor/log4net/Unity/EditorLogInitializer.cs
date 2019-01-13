using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using log4net.Unity.Config;
using UnityEditor;
using UnityEngine;

namespace log4net.Unity
{
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
        
        [InitializeOnLoadMethod]
        private static void Init()
        {
            RuntimeLogInitializer.Init();

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
            for (var i = 0; i <= Holders?.Count - 1; i++)
            {
                var holder = Holders[i];
                holder?.CheckChange();
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var exists = (importedAssets ?? Array.Empty<string>())
                .Concat(deletedAssets ?? Array.Empty<string>())
                .Concat(movedAssets ?? Array.Empty<string>())
                .Concat(movedFromAssetPaths ?? Array.Empty<string>())
                .Distinct()
                .Where(s =>
                {
                    if (string.IsNullOrEmpty(s)) return false;
                    var directory = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(s));
                    var name = Path.GetFileNameWithoutExtension(s);
                    return directory?.ToLower() == "resources" && name?.ToLower() == "log4net";
                }).Any();

            if (exists)
            {
                ResourceConfigurator.Instance.CallChange();
            }
        }
    }
}