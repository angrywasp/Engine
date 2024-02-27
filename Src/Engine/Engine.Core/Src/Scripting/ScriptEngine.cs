using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using AngryWasp.Random;

namespace Engine.Scripting
{
    public class CompilerParams
    {
        private CSharpCompilationOptions csCompilationOptions;
        private CSharpParseOptions csParseOptions;
        private List<PortableExecutableReference> references = new List<PortableExecutableReference>();

        public CSharpCompilationOptions CsCompilationOptions => csCompilationOptions;
        public CSharpParseOptions CsParseOptions => csParseOptions;
        public List<PortableExecutableReference> References => references;

        public CompilerParams(ScriptEngineSettings ses, string defines)
        {
            List<string> refs = new List<string>();

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!a.IsDynamic)
                    if (!refs.Contains(a.Location))
                        refs.Add(a.Location);
            }

            foreach (string assembly in ses.ExternalReferences)
			{
				Assembly a = ReflectionHelper.Instance.LoadAssemblyFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assembly));
                if (!refs.Contains(a.Location))
                    refs.Add(a.Location);
			}

            foreach (string assembly in ses.RuntimeReferences)
            {
                Assembly a = Assembly.Load(assembly);
                if (!refs.Contains(a.Location))
                    refs.Add(a.Location);
            }

            foreach (var r in refs)
                references.Add(MetadataReference.CreateFromFile(r));

            csCompilationOptions = new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary);

            UpdateDefines(defines);
        }

        public void UpdateDefines(string preprocessorDefines)
        {
            if (!string.IsNullOrEmpty(preprocessorDefines))
                csParseOptions = new CSharpParseOptions(preprocessorSymbols: preprocessorDefines.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries));
            else
                csParseOptions = new CSharpParseOptions();
        }

        public void AddReference(string reference)
        {
            foreach (var r in references)
                if (r.FilePath == reference)
                    return;

            references.Add(MetadataReference.CreateFromFile(reference));
        }

        public CSharpCompilation CreateCompilation(string name) =>
            CSharpCompilation.Create(name).WithOptions(csCompilationOptions).AddReferences(references);

        public List<SyntaxTree> GenerateTrees(IEnumerable<string> code)
        {
            List<SyntaxTree> trees = new List<SyntaxTree>();

            foreach (var c in code)
                trees.Add(CSharpSyntaxTree.ParseText(c, csParseOptions));

            return trees;
        }
    }

    public static class ScriptEngine
    {
		private static Dictionary<string, ClassInfo> loadedClasses = new Dictionary<string, ClassInfo>();
        private static string includes;
        private static CompilerParams compilerParams;
        public static Dictionary<string, ClassInfo> LoadedClasses => loadedClasses;

        public static void Initialize(ScriptEngineSettings ses, string defines, string precompiledScriptAssembly, bool force = false)
        {
            if (ses == null)
                ses = Settings.Engine.ScriptEngine;

            loadedClasses.Clear();
            includes = string.Empty;
			compilerParams = new CompilerParams(ses, defines);

            foreach (string ns in ses.Includes)
                includes += $"using {ns};\r\n";

            #region Build precompiled assembly

            if (!force)
            {
                if (!LoadScriptAssembly(ses.PrecompiledScriptAssembly))
                    ScriptEngine.CompileScriptAssembly(Settings.Engine.ScriptEngine.ScriptFiles, ses.PrecompiledScriptAssembly); 
            }
            else
            {
                Log.Instance.Write("Compiling script assembly");
                ScriptEngine.CompileScriptAssembly(Settings.Engine.ScriptEngine.ScriptFiles, precompiledScriptAssembly);
            }

            #endregion
        }

		public static ClassInfo GetScriptClass(string className, out Script_Result result)
        {
            if (!loadedClasses.ContainsKey(className))
            {
                result = Script_Result.ScriptNotLoaded;
                return null;
            }

            ClassInfo ci = loadedClasses[className];
            result = Script_Result.Success;

            return ci;
		}

        public static bool ExecuteCode(string code)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(includes);
            scriptBuilder.AppendLine("namespace Dynamic {");
            scriptBuilder.AppendLine("public class DynamicClass {");
            scriptBuilder.AppendLine("public static void DynamicCode() {");
            scriptBuilder.AppendLine(code);
            scriptBuilder.AppendLine("}}}");

            string formattedCode = scriptBuilder.ToString();

            var cc = compilerParams.CreateCompilation($"dll_{RandomString.AlphaNumeric(4)}").
                AddSyntaxTrees(CSharpSyntaxTree.ParseText(formattedCode, compilerParams.CsParseOptions));
            
            var ms = new MemoryStream();
            var results = cc.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);

            if (!results.Success)
            {
                foreach (var d in results.Diagnostics)
                {
                    switch (d.Severity)
                    {
                        case DiagnosticSeverity.Info: 
                            Log.Instance.Write(d.GetMessage());
                            break;
                        case DiagnosticSeverity.Warning: 
                            Log.Instance.WriteWarning(d.GetMessage());
                            break;
                        case DiagnosticSeverity.Error: 
                            Log.Instance.WriteError(d.GetMessage());
                            break;
                    }
                }

                return false;
            }

            Assembly assembly = Assembly.Load(ms.ToArray());

			try
			{
				Type classType = assembly.GetTypes()[0];
				object target = Activator.CreateInstance(classType);
				ReflectionHelper.Instance.GetMethods(classType)["DynamicCode"].Invoke(target, null);
			}
			catch (Exception ex)
			{
                Log.Instance.WriteException(ex.InnerException, "ScriptEngine Dynamic Code threw and Exception");
				return false;
			}
			return true;
        }

        public static bool ExecuteMacros(IEnumerable<string> macroFiles)
        {
            var cc = compilerParams.CreateCompilation($"dll_{RandomString.AlphaNumeric(4)}");

            foreach (string m in macroFiles)
            {
                Log.Instance.Write($"Adding Macro: {m}");
                cc = cc.AddSyntaxTrees(CSharpSyntaxTree.ParseText(File.ReadAllText(m), compilerParams.CsParseOptions));
            }
            
            var ms = new MemoryStream();
            var results = cc.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);

            if (!results.Success)
            {
                foreach (var d in results.Diagnostics)
                {
                    switch (d.Severity)
                    {
                        case DiagnosticSeverity.Info: 
                            Log.Instance.Write($"{d.Location.SourceSpan.ToString()}, {d.GetMessage()}");
                            break;
                        case DiagnosticSeverity.Warning: 
                            Log.Instance.WriteWarning($"{d.Location}, {d.GetMessage()}");
                            break;
                        case DiagnosticSeverity.Error: 
                            Log.Instance.WriteError($"{d.Location.GetLineSpan().ToString()}, {d.GetMessage()}");
                            break;
                    }
                }

                return false;
            }

            Assembly assembly = Assembly.Load(ms.ToArray());

            bool hasError = false;

            foreach (var t in ReflectionHelper.Instance.GetTypesInheritingOrImplementing(assembly, typeof(IMacro)))
            {
                IMacro target = (IMacro)Activator.CreateInstance(t);
                Log.Instance.Write($"Running macro {t}");

                try
                {
                    target.Run();
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteException(ex, $"{t}.Run() threw an Exception");
                    hasError = true;
                }
            }

            return !hasError;
        }

        public static ClassInfo LoadType(Type type, bool forceUpdate = false, XDocument documentation = null)
        {
			if (!type.IsClass)
			{
				Log.Instance.WriteWarning("Attempt to load type that is not a class");
				return null;
			}

            ScriptEngineHiddenAttribute aa = (ScriptEngineHiddenAttribute)type.GetCustomAttribute(typeof(ScriptEngineHiddenAttribute));

            if (aa != null)
				return null;

            if (!forceUpdate && loadedClasses.ContainsKey(type.FullName))
                return loadedClasses[type.FullName];

            Dictionary<string, MethodInfo> methods = ReflectionHelper.Instance.GetMethods(type).Where(x => x.Value.DeclaringType == type && 
                x.Value.GetCustomAttribute(typeof(ScriptEngineHiddenAttribute)) == null &&
                !x.Value.IsSpecialName).ToDictionary(y => y.Key, z => z.Value);

            ClassInfo ci = new ClassInfo(type, methods, type.IsAbstract && type.IsSealed, documentation);

            if (!loadedClasses.ContainsKey(type.FullName))
			{
                loadedClasses.Add(type.FullName, ci);
                if (!includes.Contains(type.Namespace))
                    includes += string.Format("using {0};\r\n", type.Namespace);
			}
            else
                if (forceUpdate)
                	loadedClasses[type.FullName] = ci;

            return ci;
		}
    
        private static bool CompileScriptAssembly(List<string> scriptFiles, string name)
        {
            Log.Instance.Write("Generating script assembly");

            #region Get list of files to compile

            List<string> codeFiles = new List<string>();

            foreach (string d in scriptFiles)
            {
                string pattern = Path.GetFileName(d);
                string dir = Path.GetDirectoryName(d);

                DirectoryInfo directory = new DirectoryInfo(EngineFolders.ContentPathVirtualToReal(dir));
                foreach (FileInfo file in directory.GetFiles(pattern, SearchOption.TopDirectoryOnly))
                {
                    Log.Instance.Write($"Adding file: {file.FullName}");
                    codeFiles.Add(File.ReadAllText(file.FullName));
                }
            }

            #endregion

            #region Compile files

            var trees = compilerParams.GenerateTrees(codeFiles);
            var cc = compilerParams.CreateCompilation($"{name}.dll").
                AddSyntaxTrees(trees).
                AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            string fullName = EngineFolders.ContentPathVirtualToReal(name);

            MemoryStream ds = new MemoryStream(), xs = new MemoryStream();

            var results = cc.Emit(peStream: ds, xmlDocumentationStream: xs);

            if (!results.Success)
            {
                foreach (var d in results.Diagnostics)
                {
                    switch (d.Severity)
                    {
                        case DiagnosticSeverity.Info: 
                            Log.Instance.Write(d.GetMessage());
                            break;
                        case DiagnosticSeverity.Warning: 
                            Log.Instance.WriteWarning(d.GetMessage());
                            break;
                        case DiagnosticSeverity.Error: 
                            Log.Instance.WriteError(d.GetMessage());
                            break;
                    }
                }
                return false;
            }

            using (BinaryWriter w = new BinaryWriter(File.OpenWrite($"{fullName}.dll")))
            {
                w.Write(ds.ToArray());
            }

            using (BinaryWriter w = new BinaryWriter(File.OpenWrite($"{fullName}.xml")))
            {
                w.Write(xs.ToArray());
            }

            Assembly assembly = ReflectionHelper.Instance.LoadAssemblyFile($"{fullName}.dll");
            XDocument documentation = XDocument.Load($"{fullName}.xml");

            #endregion

            foreach	(var t in assembly.GetTypes())
                LoadType(t, true, documentation);

            compilerParams.AddReference(assembly.Location);

            Log.Instance.Write($"Compiled assembly located at: {assembly.Location}");

            return true;
        }

        private static bool LoadScriptAssembly(string assemblyFile)
        {
            Log.Instance.Write($"Using precompiled script assembly: {assemblyFile}");

            string dllPath = string.IsNullOrEmpty(assemblyFile) ? string.Empty : EngineFolders.ContentPathVirtualToReal(assemblyFile);
            string xmlPath = Path.ChangeExtension(dllPath, ".xml");

            if (!File.Exists(dllPath))
                return false;

            if (!File.Exists(xmlPath))
                Log.Instance.WriteWarning("Could not find XML documentation. Help will be unavailable");

            Assembly assembly = ReflectionHelper.Instance.LoadAssemblyFile(dllPath);
            XDocument documentation = XDocument.Load(xmlPath);

            compilerParams.AddReference(dllPath);

            foreach	(var t in assembly.GetTypes())
            {
                if (LoadType(t, true, documentation) == null)
                    Log.Instance.WriteWarning($"Could not load type {t}. Type information will be unavailable");
            }

            return true;
        }
    }
}
