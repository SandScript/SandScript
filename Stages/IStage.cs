using System;

namespace SandScript;

public interface IStage
{
	StageDiagnostics Diagnostics { get; }
	Type? PrerequisiteStage { get; }
	Type? SortBeforeStage { get; }

	StageResult Run( Script owner, object?[] arguments );
}
