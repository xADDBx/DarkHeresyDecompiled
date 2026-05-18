using System.Collections.Generic;

namespace Kingmaker.Framework.ContextContract;

public sealed class ValidationIssue
{
	public enum SeverityLevel
	{
		Warning,
		Error
	}

	public SeverityLevel Severity { get; }

	public string Message { get; }

	public object OwnerElement { get; }

	public IReadOnlyList<PathStep> Path { get; }

	public ContextField Field { get; }

	public Availability RequiredLevel { get; }

	public Availability ActualLevel { get; }

	public ValidationIssue(SeverityLevel severity, string message, object ownerElement, IReadOnlyList<PathStep> path, ContextField field, Availability requiredLevel, Availability actualLevel)
	{
		Severity = severity;
		Message = message;
		OwnerElement = ownerElement;
		Path = path;
		Field = field;
		RequiredLevel = requiredLevel;
		ActualLevel = actualLevel;
	}
}
