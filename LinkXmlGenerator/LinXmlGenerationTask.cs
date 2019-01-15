using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Mono.Cecil;

namespace LinXmlGeneration
{
    public class LinXmlGenerationTask: ITask
    {
        private string target;
        [Required]
        public string Target
        {
            get => target;
            set => target = value;
        }
        
        private string dlls;
        [Required]
        public string Dlls
        {
            get => dlls;
            set => dlls = value;
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
                $"Execute for: '{target}', with assemblies '{dlls}'", string.Empty, nameof(LinXmlGenerationTask), MessageImportance.High));
            
            var assemblies = new List<AssemblyDefinition>();

            var dllPaths = dlls
                .Split(';')
                .Select(s => s?.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Where(File.Exists).ToArray();


            for (var i = 0; i <= dllPaths.Length - 1; i++)
            {
                try
                {
                    assemblies.Add(AssemblyDefinition.ReadAssembly(dllPaths[i]));
                }
                catch
                {
                    engine.LogMessageEvent(new BuildMessageEventArgs(
                        $"Error occured while try load assembly from '{dllPaths[i]}'", string.Empty, nameof(LinXmlGenerationTask), MessageImportance.High));
                }
            }

            try
            {
                Generation.Generate(target, assemblies.ToArray());
            }
            finally
            {
                for (var i = 0; i <= assemblies.Count - 1; i++)
                {
                    assemblies[i]?.Dispose();
                }
            }
            return true;
        }
    }
}