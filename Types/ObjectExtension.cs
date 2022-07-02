namespace SandScript;

public static class ObjectExtension
{
	public static ITypeProvider? GetTypeProvider( this object? obj ) => obj is null
		? TypeProviders.Builtin.Nothing
		: TypeProviders.GetByType( obj.GetType() );
}
