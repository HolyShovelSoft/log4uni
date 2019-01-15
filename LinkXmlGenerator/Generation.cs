using System.IO;
using System.Xml;
using Mono.Cecil;

namespace LinXmlGeneration
{
    internal static class Generation
    {
        private static void ProcessType(XmlDocument doc, XmlElement element, TypeDefinition type)
        {
            if(type.FullName == "<Module>") return;
            var typeNode = doc.CreateElement("", "type", "");
            typeNode.SetAttribute("fullname", type.FullName);
            typeNode.SetAttribute("preserve", "all");
            element.AppendChild(typeNode);
        }
        
        private static void ProcessAssembly(XmlDocument doc, XmlElement element, AssemblyDefinition assembly)
        {
            var asmNode = doc.CreateElement("", "assembly", "");
            asmNode.SetAttribute("fullname", assembly.Name.Name);
            asmNode.SetAttribute("preserve", "all");
            element.AppendChild(asmNode);
            var types = assembly.MainModule.GetTypes();
            
            foreach (var type in types)
            {
                ProcessType(doc, asmNode, type);
            }
        }
        
        
        public static void Generate(string output, AssemblyDefinition[] assemblies)
        {
            var file = output;
            
            var xmlDoc = new XmlDocument();
            var link = xmlDoc.CreateElement( "", "linker", "" );
            xmlDoc.AppendChild( link );
            
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration( "1.0", "UTF-8", null );
            xmlDoc.InsertBefore( xmlDeclaration, link );

            for (var i = 0; i <= assemblies.Length - 1; i++)
            {
                ProcessAssembly(xmlDoc, link, assemblies[i]);
            }

            if (File.Exists(file))
            {
                File.Delete(file);
            }
            
            xmlDoc.Save(file);
        }
    }
}