using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SandScript;

public sealed class ScriptVariable
{
	public readonly string Name;
	public readonly ITypeProvider TypeProvider;

	public readonly bool CanRead;
	public readonly bool CanWrite;
	
	private readonly bool _isProperty;
	
	[MemberNotNullWhen(true, nameof(_isProperty))]
	private PropertyInfo? Property { get; }
	[MemberNotNullWhen(false, nameof(_isProperty))]
	private FieldInfo? Field { get; }

	internal ScriptVariable( PropertyInfo propertyInfo, ScriptVariableAttribute attribute )
	{
		Name = attribute.VariableName;
		Property = propertyInfo;
		TypeProvider = TypeProviders.GetByType( propertyInfo.PropertyType )!;

		CanRead = attribute.CanRead;
		CanWrite = attribute.CanWrite;
		
		_isProperty = true;
	}

	internal ScriptVariable( FieldInfo fieldInfo, ScriptVariableAttribute attribute )
	{
		Name = attribute.VariableName;
		Field = fieldInfo;
		TypeProvider = TypeProviders.GetByType( fieldInfo.FieldType )!;
		
		CanRead = attribute.CanRead;
		CanWrite = attribute.CanWrite;

		_isProperty = false;
	}

	public object? GetValue() => _isProperty
		? Property!.GetGetMethod()!.Invoke( null, null )
		: Field!.GetValue( null );

	public void SetValue( object? value )
	{
		if ( _isProperty )
			Property!.GetSetMethod()!.Invoke( null, new[] {value} );
		else
			Field!.SetValue( null, value );
	}
}
