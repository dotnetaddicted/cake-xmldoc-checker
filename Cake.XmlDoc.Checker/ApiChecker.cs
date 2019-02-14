using Cake.Core.Diagnostics;
using Cake.XmlDoc.Checker.DocsByReflection;
using Cake.XmlDoc.Checker.Dto;
using System;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Cake.XmlDoc.Checker
{
    public static class ApiChecker
    {
        private static string MISSING_XML = "<MISSING>";

       public static ICakeLog Log { get; set; }

       public static ApiCheckResult Check(CheckOptions checkOptions)
       {
            var result = new ApiCheckResult();          

            var assemblyPath = checkOptions.AssemblyPath;
            Assembly assembly = Assembly.LoadFile(assemblyPath);

            result.AssemblyName = assembly.FullName;
            
            Type[] allTypes = new Type[] { };
            try
            {
                allTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Swallow
                allTypes = ex.Types;
            }

            allTypes = allTypes.Where(_ => _ != null && _.IsPublic).ToArray();

            result.Namespaces = allTypes.Select(_ => new NamespaceDto { Name = _.Namespace }).Distinct().ToList();
            var nsCount = result.Namespaces.Count();

            Log.Write(Verbosity.Normal, LogLevel.Information, "Total namespaces found: {0}", nsCount);

            for (int i = 0; i < nsCount; i++)
            {
                var nsDto = result.Namespaces[i];                

                var types = allTypes.Where(_ => _.Namespace == nsDto.Name).ToList();
                var typesCount = types.Count();
                
                Log.Write(Verbosity.Normal, LogLevel.Information, "Processing namespace {0} of {1}. Namespace: '{2}'. Types: {3}", i + 1, nsCount, nsDto.Name, typesCount);

                // PROCESS NAMESPACE                
                var nsXml = DocsService.GetXmlForNamespace(nsDto.Name, allTypes[0], false);
                nsDto.XmlDescription = GetInnerText(nsXml);
                
                if (string.IsNullOrEmpty(nsDto.XmlDescription))
                {
                    Log.Write(Verbosity.Normal, LogLevel.Warning, "NAMESPACE: {0}, XML: {1}", nsDto.Name, MISSING_XML);
                }
                else if (!checkOptions.LogOnlyMissing)
                {
                    Log.Write(Verbosity.Normal, LogLevel.Information, "NAMESPACE: {0}, XML: {1}", nsDto.Name, nsDto.XmlDescription);
                }

                for (int j = 0; j < typesCount; j++)
                {
                    var t = types[j];
                    var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    var constructorsCount = constructors.Count();

                    var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Where(_ => _.Name != "value__").ToList();
                    var fieldsCount = fields.Count();

                    var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    var propertiesCount = properties.Count();

                    var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Where(_ => !_.IsSpecialName)
                        .Where(_ => !_.IsGenericMethod)
                        .ToList();
                    var methodsCount = methods.Count();

                    Log.Write(Verbosity.Normal, LogLevel.Information, "  Processing type {0} of {1}. TypeName: '{2}'. Constructors: {3}, Methods: {4}, Fields: {5}, Properties {6}",
                        j + 1, typesCount, t.Name, constructorsCount, methodsCount, fieldsCount, propertiesCount);

                    var xmlType = DocsService.GetXmlFromType(t, false);

                    // Type
                    var typeDto = new TypeDto { Name = t.Name, XmlDescription = GetInnerText(xmlType) };
                    nsDto.Types.Add(typeDto);

                    if (string.IsNullOrEmpty(typeDto.XmlDescription))
                    {
                        Log.Write(Verbosity.Normal, LogLevel.Warning, "TYPE: {0}.{1}, XML: {2}", nsDto.Name, typeDto.Name, MISSING_XML);
                    }
                    else if (!checkOptions.LogOnlyMissing)
                    {
                        Log.Write(Verbosity.Normal, LogLevel.Information, "TYPE: {0}.{1}, XML: {2}", nsDto.Name, typeDto.Name, typeDto.XmlDescription);
                    }

                    if(IsDelegate(t.UnderlyingSystemType) && checkOptions.SkipDelegatesProcessing)
                    {
                        // Skip delegates processing
                        continue;
                    }

                    // CONSTRUCTORS
                    ProcessConstructors(checkOptions, constructors, constructorsCount, typeDto);

                    // METHODS                    
                    ProcessMethods(checkOptions, methods, methodsCount, typeDto);

                    // FIELDS                      
                    ProcessFields(checkOptions, fields, fieldsCount, typeDto);

                    // PROPERTIES                    
                    ProcessProperties(checkOptions, properties, propertiesCount, typeDto);
                }
            }

            // LOG RESULTS
            if (result.HasErrors)
            {
                Log.Write(Verbosity.Normal, LogLevel.Error, "RESULT: Check completed with {0} errors", result.ErrorCount);
            }
            else
            {
                Log.Write(Verbosity.Normal, LogLevel.Information, "RESULT: Check completed successfully!");
            }

            return result;
        }

        private static void ProcessConstructors(CheckOptions checkOptions, ConstructorInfo[] constructors, int constructorsCount, TypeDto typeDto)
        {
            for (int n = 0; n < constructorsCount; n++)
            {
                var cInfo = constructors[n];

                var cDto = new ConstructorDto { Name = cInfo.ToString() };
                typeDto.Constructors.Add(cDto);

                if (!checkOptions.LogOnlyMissing)
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    Processing constructor {0} of {1}", n + 1, constructorsCount);

                var constructorXml = DocsService.GetXmlForConstructor(cInfo, false);
                cDto.XmlDescription = GetInnerText(constructorXml);

                if (string.IsNullOrEmpty(cDto.XmlDescription) && cInfo.GetParameters().Count() == 0)
                    cDto.XmlDescription = "Default constructor is allowed to have empty summary description";

                if (string.IsNullOrEmpty(cDto.XmlDescription))
                {
                    Log.Write(Verbosity.Normal, LogLevel.Warning, "    CONSTRUCTOR: {0} {1} XML: {2}", typeDto.Name, cDto.Name, MISSING_XML);
                }
                else if (!checkOptions.LogOnlyMissing)
                {
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    CONSTRUCTOR: {0} {1} XML: {2}", typeDto.Name, cDto.Name, cDto.XmlDescription);
                }
            }
        }

        private static void ProcessMethods(CheckOptions checkOptions, System.Collections.Generic.List<MethodInfo> methods, int methodsCount, TypeDto typeDto)
        {
            for (int n = 0; n < methodsCount; n++)
            {
                var mInfo = methods[n];
                var mDto = new MethodDto { Name = mInfo.ToString() };
                typeDto.Methods.Add(mDto);

                if (!checkOptions.LogOnlyMissing)
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    Processing method {0} of {1}", n + 1, methodsCount);

                var methodXml = DocsService.GetXmlFromMember(mInfo, false);
                mDto.XmlDescription = GetInnerText(methodXml);

                if (string.IsNullOrEmpty(mDto.XmlDescription))
                {
                    Log.Write(Verbosity.Normal, LogLevel.Warning, "    METHOD: {0} XML: {1}", mDto.Name, MISSING_XML);
                }
                else if (!checkOptions.LogOnlyMissing)
                {
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    METHOD: {0} XML: {1}", mDto.Name, mDto.XmlDescription);
                }
            }
        }

        private static void ProcessProperties(CheckOptions checkOptions, PropertyInfo[] properties, int propertiesCount, TypeDto typeDto)
        {
            for (int n = 0; n < propertiesCount; n++)
            {
                var pInfo = properties[n];
                var pDto = new PropertyDto { Name = pInfo.ToString() };
                typeDto.Properties.Add(pDto);

                if (!checkOptions.LogOnlyMissing)
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    Processing property {0} of {1}", n + 1, propertiesCount);

                var propertyDocXml = DocsService.GetXmlFromMember(pInfo, false);
                pDto.XmlDescription = GetInnerText(propertyDocXml);

                if (string.IsNullOrEmpty(pDto.XmlDescription))
                {
                    Log.Write(Verbosity.Normal, LogLevel.Warning, "    PROPERTY: {0} XML: {1}", pDto.Name, MISSING_XML);
                }
                else if (!checkOptions.LogOnlyMissing)
                {
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    PROPERTY: {0} XML: {1}", pDto.Name, pDto.XmlDescription);
                }
            }
        }

        private static void ProcessFields(CheckOptions checkOptions, System.Collections.Generic.List<FieldInfo> fields, int fieldsCount, TypeDto typeDto)
        {
            for (int n = 0; n < fieldsCount; n++)
            {
                var fInfo = fields[n];
                var fDto = new FieldDto { Name = fInfo.ToString() };
                typeDto.Fields.Add(fDto);

                if (!checkOptions.LogOnlyMissing)
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    Processing field {0} of {1}", n + 1, fieldsCount);

                var fieldDocXml = DocsService.GetXmlFromMember(fInfo, false);
                fDto.XmlDescription = GetInnerText(fieldDocXml);

                if (string.IsNullOrEmpty(fDto.XmlDescription))
                {
                    Log.Write(Verbosity.Normal, LogLevel.Warning, "    FIELD: {0} XML: {1}", fDto.Name, MISSING_XML);
                }
                else if (!checkOptions.LogOnlyMissing)
                {
                    Log.Write(Verbosity.Normal, LogLevel.Information, "    FIELD: {0} XML: {1}", fDto.Name, fDto.XmlDescription);
                }
            }
        }

        private static string GetInnerText(XmlElement xmlElement)
        {
            return xmlElement != null ? xmlElement.SelectSingleNode("summary").InnerText.Trim() : string.Empty;
        }

        private static bool IsDelegate(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type.BaseType);
        }
    }
}
