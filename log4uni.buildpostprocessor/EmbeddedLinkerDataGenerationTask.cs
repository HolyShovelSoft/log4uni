using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace log4uni.BuildPostprocessor
{
    public class EmbeddedLinkerDataGenerationTask: ITask
    {
        
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
                $"[{nameof(EmbeddedLinkerDataGenerationTask)}] Execute for assemblies '{dlls}'", string.Empty, nameof(EmbeddedLinkerDataGenerationTask), MessageImportance.High));
            
            var assemblies = new List<AssemblyDefinition>();

            var dllPaths = dlls
                .Split(';')
                .Select(s => s?.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Where(File.Exists).ToArray();

            var files = new List<FileStream>();
            
            for (var i = 0; i <= dllPaths.Length - 1; i++)
            {
                try
                {
                    var file = new FileStream(dllPaths[i], FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    assemblies.Add(AssemblyDefinition.ReadAssembly(file));
                    files.Add(file);
                }
                catch
                {
                    engine.LogMessageEvent(new BuildMessageEventArgs(
                        $"[{nameof(EmbeddedLinkerDataGenerationTask)}] Error occured while try load assembly from '{dllPaths[i]}'", string.Empty, nameof(EmbeddedLinkerDataGenerationTask), MessageImportance.High));
                }
            }

            try
            {
                Generate(assemblies.ToArray());
            }
            finally
            {
                for (var i = 0; i <= assemblies.Count - 1; i++)
                {
                    try
                    {
                        assemblies[i]?.Dispose();
                    }
                    catch 
                    {
                        //
                    }
                }

                for (var i = 0; i <= files.Count - 1; i++)
                {
                    try
                    {
                        files[i].Dispose();
                    }
                    catch
                    {
                        //
                    }
                }
            }
            return true;
        }

        private static void WritePreserveToMethod(MethodDefinition methodDefinition, MethodDefinition ctorDefinition)
        {
            methodDefinition.CustomAttributes.Add(new CustomAttribute(ctorDefinition));
        }
        
        private static void WritePreserveToEvent(EventDefinition eventDefinition, MethodDefinition ctorDefinition)
        {
            eventDefinition.CustomAttributes.Add(new CustomAttribute(ctorDefinition));
        }
        
        private static void WritePreserveToField(FieldDefinition fieldDefinition, MethodDefinition ctorDefinition)
        {
            fieldDefinition.CustomAttributes.Add(new CustomAttribute(ctorDefinition));
        }
        
        private static void WritePreserveToProperty(PropertyDefinition propertyDefinition, MethodDefinition ctorDefinition)
        {
            propertyDefinition.CustomAttributes.Add(new CustomAttribute(ctorDefinition));
        }
        
        private static void WritePreserveToType(TypeDefinition type, TypeDefinition preserveAttributeType)
        {
            var ctorMethod = preserveAttributeType.GetConstructors()
                .First(definition => definition.Parameters.Count == 0);
            
            type.CustomAttributes.Add(new CustomAttribute(ctorMethod));

            foreach (var method in type.Methods)
            {
                WritePreserveToMethod(method, ctorMethod);
            }
            foreach (var evt in type.Events)
            {
                WritePreserveToEvent(evt, ctorMethod);
            }
            foreach (var property in type.Properties)
            {
                WritePreserveToProperty(property, ctorMethod);
            }
            foreach (var field in type.Fields)
            {
                WritePreserveToField(field, ctorMethod);
            }
        }
        
        private static void ProcessType(XmlDocument doc, XmlElement element, TypeDefinition type, TypeDefinition preserveAttributeType)
        {
            if(type.FullName == "<Module>") return;
            var typeNode = doc.CreateElement("", "type", "");
            typeNode.SetAttribute("fullname", type.FullName);
            typeNode.SetAttribute("preserve", "all");
            element.AppendChild(typeNode);

            if (preserveAttributeType != null)
            {
                WritePreserveToType(type, preserveAttributeType);
            }
        }
        
        private static void ProcessAssembly(XmlDocument doc, XmlElement element, AssemblyDefinition assembly)
        {
            var asmNode = doc.CreateElement("", "assembly", "");
            asmNode.SetAttribute("fullname", assembly.Name.Name);
            asmNode.SetAttribute("preserve", "all");
            element.AppendChild(asmNode);
            
            var types = assembly.MainModule.Types;
            var preserveAttributeType = assembly.MainModule.Types.FirstOrDefault(definition =>
                string.IsNullOrWhiteSpace(definition.Namespace) && definition.Name == "PreserveAttribute");
            foreach (var type in types)
            {
                ProcessType(doc, asmNode, type, preserveAttributeType);
            }
        }
        
        
        public static void Generate(AssemblyDefinition[] assemblies)
        {
            for (var i = 0; i <= assemblies.Length - 1; i++)
            {
                var xmlDoc = new XmlDocument();
                var link = xmlDoc.CreateElement( "", "linker", "" );
                xmlDoc.AppendChild( link );
            
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration( "1.0", "UTF-8", null );
                xmlDoc.InsertBefore( xmlDeclaration, link );

                var asm = assemblies[i];
                
                ProcessAssembly(xmlDoc, link, asm);

                var fileName = asm.Name.Name;

                var resource = asm.MainModule.Resources.FirstOrDefault(r =>
                    r.Name == $"{Path.GetFileNameWithoutExtension(fileName)}.xml");

                if (resource != null)
                {
                    asm.MainModule.Resources.Remove(resource);
                }

                var content = xmlDoc.InnerXml;

                resource = new EmbeddedResource($"{Path.GetFileNameWithoutExtension(fileName)}.xml",
                    ManifestResourceAttributes.Public, Encoding.UTF8.GetBytes(content));
                
                asm.MainModule.Resources.Add(resource);
                
                asm.Write();
            }
        }
    }
}