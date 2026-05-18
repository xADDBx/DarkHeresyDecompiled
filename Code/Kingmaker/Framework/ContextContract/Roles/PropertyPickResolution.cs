using JetBrains.Annotations;
using Kingmaker.EntitySystem.Properties;

namespace Kingmaker.Framework.ContextContract.Roles;

public readonly struct PropertyPickResolution
{
	[NotNull]
	public string OwnerTypeName { get; }

	[NotNull]
	public string FieldName { get; }

	public PropertyTargetType Picked { get; }

	public ContextField Primary { get; }

	[NotNull]
	public ContextField[] Chain { get; }

	public ContextField? ResolvedTo { get; }

	public bool ResolvedViaFallback
	{
		get
		{
			if (ResolvedTo.HasValue)
			{
				return ResolvedTo.Value != Primary;
			}
			return false;
		}
	}

	public bool Unresolved => !ResolvedTo.HasValue;

	public PropertyPickResolution([NotNull] string ownerTypeName, [NotNull] string fieldName, PropertyTargetType picked, ContextField primary, [NotNull] ContextField[] chain, ContextField? resolvedTo)
	{
		OwnerTypeName = ownerTypeName;
		FieldName = fieldName;
		Picked = picked;
		Primary = primary;
		Chain = chain;
		ResolvedTo = resolvedTo;
	}
}
