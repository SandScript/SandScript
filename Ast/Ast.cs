﻿namespace SandScript.AbstractSyntaxTrees;

public abstract class Ast
{
	public readonly TokenLocation StartLocation;

	protected Ast( TokenLocation startLocation )
	{
		StartLocation = startLocation;
	}
}
