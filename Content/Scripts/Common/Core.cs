using System;
using System.Reflection;
using AngryWasp.Logger;
using Engine.Helpers;
using Engine.Scripting;

namespace EngineScripting
{
    /// <summary>
    /// This is a summary for the Term class
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// Lists all the script classes available
        /// </summary>
        public static void ListClasses()
        {
            if (ScriptEngine.LoadedClasses.Count == 0)
            {
                Log.Instance.WriteError("No scripts loaded...");
                return;
            }

            foreach (string className in ScriptEngine.LoadedClasses.Keys)
                Log.Instance.Write(className);
        }

        /// <summary>
        /// Lists the available methods in a script class
        /// </summary>
        /// <param name="type">The type of the script class to check</param>
        public static void ListMethods(Type type)
        {
            Script_Result scriptResult;
            ClassInfo ci = ScriptEngine.GetScriptClass(type.Name, out scriptResult);

            if (scriptResult == Script_Result.ScriptNotLoaded)
            {
                Log.Instance.WriteError($"Script class '{type.Name}' not loaded.");
                return;
            }

            List(ci);
        }

        public static void Help(string name)
        {
            string[] data = name.Split('.');
            string typeName = data[0];
            string methodName = data[1];
            Script_Result scriptResult;
            ClassInfo ci = ScriptEngine.GetScriptClass(typeName, out scriptResult);
            if (scriptResult != Script_Result.Success)
            {
                Log.Instance.WriteError("Could not find class");
            }

            if (ci.Documentation.ContainsKey(methodName))
            {
                ClassDocumentationInfo cdi = ci.Documentation[methodName];
                Log.Instance.SetColor(ConsoleColor.DarkCyan);
                Log.Instance.Write(cdi.Summary);
                Log.Instance.SetColor(ConsoleColor.White);
            }
            else
                Log.Instance.WriteWarning("No help available");
        }

        /// <summary>
        /// Lists all available script classes and their methods
        /// </summary>
        public static void ListAll()
        {
            foreach (var i in ScriptEngine.LoadedClasses)
            {
                Log.Instance.SetColor(ConsoleColor.Green);
                Log.Instance.Write(i.Key);
                Log.Instance.SetColor(ConsoleColor.White);
                List(i.Value);
            }
        }

        /// <summary>
        /// Runs a macro at the specified path
        /// </summary>
        /// <param name="macro">The path to the macro to run</param>
        /// <remarks>
        /// Macros must be stored in the content directory and the path supplied relative to the content directory
        /// </remarks>
        public static void RunMacro(string macro)
        {
#pragma warning disable CS4014
            ScriptEngine.ExecuteMacros(new string[] { EngineFolders.ContentPathVirtualToReal(macro) });
#pragma warning restore CS4014
        }

        #region Private methods

        private static void List(ClassInfo ci)
        {
            foreach (MethodInfo method in ci.Methods.Values)
                List(method);
        }

        private static void List(MethodInfo method)
        {
            ParameterInfo[] pi = method.GetParameters();

            string methodName = method.Name;

            if (method.IsGenericMethod)
            {
                Type[] gt = method.GetGenericArguments();

                methodName += "<";

                for (int i = 0; i < gt.Length; i++)
                {
                    methodName += gt[i].BaseType.Name;

                    if (i != gt.Length - 1)
                        methodName += ", ";
                }

                methodName += ">";
            }

            string signature = "(";

            if (pi.Length > 0)
            {
                for (int i = 0; i < pi.Length; i++)
                {
                    signature += (pi[i].IsOut) ? "out " : string.Empty;
                    signature += string.Format("{0} {1}", pi[i].ParameterType.Name, pi[i].Name);

                    if (i != pi.Length - 1)
                        signature += ", ";
                }
            }

            signature += ")";

            Log.Instance.Write($"\t{method.ReturnType.Name} {methodName}{signature}");
        }

        #endregion
    }
}