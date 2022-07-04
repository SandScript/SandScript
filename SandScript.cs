using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SandScript.Exceptions;

namespace SandScript;

public static class SandScript
{
	public static IEnumerable<ScriptMethod> CustomMethods => CustomMethodCache;
	private static readonly List<ScriptMethod> CustomMethodCache = new();
	public static IEnumerable<ScriptVariable> CustomVariables => CustomVariableCache;
	private static readonly List<ScriptVariable> CustomVariableCache = new();

	public static void RegisterClassAttributes<T>() where T : class
	{
		RegisterClassMethods<T>();
		RegisterClassVariables<T>();
	}

	public static void RegisterClassMethods<T>() where T : class
	{
		var methods = typeof(T).GetMethods( BindingFlags.Public | BindingFlags.Static )
			.Where( m => m.GetCustomAttributes( typeof(ScriptMethodAttribute), false ).Length > 0 );
		
		foreach ( var method in methods )
		{
			if ( TypeProviders.GetByType( method.ReturnType ) is null )
				throw new ReturnTypeUnsupportedException( method.ReturnType );
					
			var methodParams = method.GetParameters();
			if ( methodParams.Length == 0 || methodParams[0].ParameterType != typeof(Script) )
				throw new ParameterException( "First parameter must be of type " + nameof(Script) + "." );

			for ( var i = 1; i < methodParams.Length; i++ )
			{
				var methodParam = methodParams[i];
				if ( methodParam.ParameterType != typeof(ScriptValue) &&
				     TypeProviders.GetByType( method.ReturnType ) is null )
					throw new ParameterException( "Parameter type \"" + methodParam.ParameterType + "\" is unsupported." );
			}
			
			foreach ( var attribute in method.GetCustomAttributes<ScriptMethodAttribute>() )
				CustomMethodCache.Add( new ScriptMethod( method, attribute ) );
		}
	}

	public static void RegisterClassVariables<T>() where T : class
	{
		var properties = typeof(T).GetProperties( BindingFlags.Public | BindingFlags.Static )
			.Where( p => p.GetCustomAttributes( typeof(ScriptVariableAttribute), false ).Length > 0 );

		foreach ( var property in properties )
		{
			if ( TypeProviders.GetByType( property.PropertyType ) is null )
				throw new TypeUnsupportedException( property.PropertyType );

			foreach ( var attribute in property.GetCustomAttributes<ScriptVariableAttribute>() )
			{
				if ( attribute.CanRead && !property.CanRead )
					throw new UnreadableVariableException( property, attribute );

				if ( attribute.CanWrite && !property.CanWrite )
					throw new UnwritableVariableException( property, attribute );
				
				CustomVariableCache.Add( new ScriptVariable( property, attribute ) );
			}
		}

		var fields = typeof(T).GetFields( BindingFlags.Public | BindingFlags.Static )
			.Where( f => f.GetCustomAttributes( typeof(ScriptVariableAttribute), false ).Length > 0 );

		foreach ( var field in fields )
		{
			if ( TypeProviders.GetByType( field.FieldType ) is null )
				throw new TypeUnsupportedException( field.FieldType );

			foreach ( var attribute in field.GetCustomAttributes<ScriptVariableAttribute>() )
			{
				if ( attribute.CanWrite && field.IsLiteral || field.IsInitOnly )
					throw new UnwritableVariableException( field, attribute );
				
				CustomVariableCache.Add( new ScriptVariable( field, attribute ) );
			}
		}
	}
}
