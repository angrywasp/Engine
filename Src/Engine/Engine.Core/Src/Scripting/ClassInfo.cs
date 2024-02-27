using System;
using System.Collections.Generic;
using System.Reflection;
using AngryWasp.Helpers;
using System.Xml.Linq;

namespace Engine.Scripting
{
    /// <summary>
    /// Allows a script author to hide public classes or methods from being enumerated in the script engine
    /// </summary>
    public sealed class ScriptEngineHiddenAttribute : Attribute
	{
		public ScriptEngineHiddenAttribute() { }
	}

	public interface IMacro
	{
		void Run();
	}

	public class ClassDocumentationInfo
	{
		private string summary;
		private string remarks;
		private KeyValuePair<string, string>[] parameters;

		public string Summary => summary;

		public string Remarks => remarks;

		public KeyValuePair<string, string>[] Parameters => parameters;

		public ClassDocumentationInfo(string summary, KeyValuePair<string, string>[] parameters, string remarks)
		{
			this.summary = summary;
			this.parameters = parameters;
			this.remarks = remarks;
		}
	}

	public class ClassInfo
	{
		private Type classType;
		private Dictionary<string, MethodInfo> methods;
		private Dictionary<string, ClassDocumentationInfo> docs;
		private bool isStatic;

		public Type ClassType => classType;

		public Dictionary<string, MethodInfo> Methods => methods;
		public Dictionary<string, ClassDocumentationInfo> Documentation => docs;

		public ClassInfo(Type classType, Dictionary<string, MethodInfo> methods, bool isStatic, XDocument documentation)
		{
			this.classType = classType;
			this.methods = methods;
			this.isStatic = isStatic;

			if (documentation != null)
			{
				List<XElement> elements = XHelper.GetNodesByName(documentation.Root, "members.member");
				docs = new Dictionary<string, ClassDocumentationInfo>();

				foreach (var method in methods)
				{
					string elementName = string.Format("{0}.{1}", classType.Name, method.Key);

					foreach (var element in elements)
					{

						string s = XHelper.GetAttribute(element, "name").Substring(2);

						int i1 = s.IndexOf('(');
						int i2 = s.IndexOf('`');
						if (i1 != -1 || i2 != -1)
						{
							if (i1 == -1 && i2 != -1)
								s = s.Substring(0, i2);
							else if (i1 != -1 && i2 == -1)
								s = s.Substring(0, i1);
							else
                                s = s.Substring(0, System.Math.Min(i1, i2));
						}

						if (s == elementName)
						{
							string summary = XHelper.GetNodeByName(element, "summary").Value.Trim();
							var r = XHelper.GetNodeByName(element, "remarks");
							string remarks = null;
							if (r != null)
								remarks = r.Value.Trim();
							List<XElement> p = XHelper.GetNodesByName(element, "param");
							KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[p.Count];

							for	(int i = 0; i < p.Count; i++)
								parameters[i] = new KeyValuePair<string, string>(p[i].Attribute("name").Value.Trim(), p[i].Value.Trim());

							docs.Add(method.Key, new ClassDocumentationInfo(summary, parameters, remarks));
							continue;
						}
					}
				}
			}
		}
	}
}
