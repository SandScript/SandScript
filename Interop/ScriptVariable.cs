using System.Reflection;

namespace SandScript;

public sealed class ScriptVariable
{
	public readonly string Name;
	public readonly ITypeProvider TypeProvider;

	public readonly bool CanRead;
	public readonly bool CanWrite;
	
	private PropertyInfo Property { get; }

	internal ScriptVariable( PropertyInfo propertyInfo, ScriptVariableAttribute attribute )
	{
		Name = attribute.VariableName;
		Property = propertyInfo;
		TypeProvider = TypeProviders.GetByBackingType( propertyInfo.PropertyType )!;

		CanRead = attribute.CanRead;
		CanWrite = attribute.CanWrite;
	}

	public object? GetValue()
	{
		return Property.GetGetMethod()!.Invoke( null, null );
	}

	public void SetValue( object? value )
	{
		Property.GetSetMethod()!.Invoke( null, new[] {value} );
	}
}
