namespace Engine.Scripting
{
	public enum Method_Result
	{
		Success,
		ClassNameInvalid,
		MethodNameInvalid,
		MethodThrewException,
	}

	public enum Script_Result
	{
		FileNotFound,
		NoClassesInScript,
		ScriptNotLoaded,
		Success
	}
}
