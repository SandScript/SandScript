using JetBrains.Annotations;

namespace SandScript;

[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
[MeansImplicitUse]
public class ScriptMethodAttribute : Attribute
{
	public readonly string MethodName;

	public ScriptMethodAttribute( string methodName )
	{
		MethodName = methodName;
	}
}
