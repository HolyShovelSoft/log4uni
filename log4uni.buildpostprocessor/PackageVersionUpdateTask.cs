using System;
using System.IO;
using Microsoft.Build.Framework;
using Mono.Cecil;

namespace log4uni.BuildPostprocessor
{
    public class PackageVersionUpdateTask: ITask
    {
        private string targetJson;
        [Required]
        public string TargetJson
        {
            get => targetJson;
            set => targetJson = value;
        }
        
        private string targetAssembly;
        [Required]
        public string TargetAssembly
        {
            get => targetAssembly;
            set => targetAssembly = value;
        }
        
        private IBuildEngine engine;               
        public IBuildEngine BuildEngine
        {
            get { return engine; }
            set { engine = value; }
        }       


        private ITaskHost host;
        public ITaskHost HostObject
        {
            get { return host; }
            set { host = value; }
        }
        
        
        public bool Execute()
        {
            engine.LogMessageEvent(new BuildMessageEventArgs(
                $"[{nameof(PackageVersionUpdateTask)}] Execute for: '{TargetJson}', with assembly '{TargetAssembly}'", string.Empty, nameof(PackageVersionUpdateTask), MessageImportance.High));

            if (string.IsNullOrWhiteSpace(TargetJson) || string.IsNullOrWhiteSpace(TargetAssembly) ||
                !File.Exists(TargetJson) || !File.Exists(TargetAssembly))
            {
                engine.LogMessageEvent(new BuildMessageEventArgs(
                    $"[{nameof(PackageVersionUpdateTask)}] Error detecting processing files", string.Empty, nameof(PackageVersionUpdateTask), MessageImportance.High));
                return false;
            }

            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(TargetAssembly);
                var version = assembly.Name.Version;
                assembly.Dispose();
                var text = File.ReadAllText(TargetJson);
                text = text.Replace("[version]", $"{version.Major}.{version.Minor}.{version.Build}");
                File.WriteAllText(targetJson, text);
            }
            catch (Exception e)
            {
                engine.LogMessageEvent(new BuildMessageEventArgs(
                    $"[{nameof(PackageVersionUpdateTask)}] Error occured while try process package version '{e.Message}'", string.Empty, nameof(PackageVersionUpdateTask), MessageImportance.High));
                return false;
            }
            
            return true;
        }
    }
}