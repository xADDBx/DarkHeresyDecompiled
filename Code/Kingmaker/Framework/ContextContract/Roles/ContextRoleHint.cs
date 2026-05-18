using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract.Roles;

public readonly struct ContextRoleHint
{
	private static readonly IReadOnlyList<ContextRoleFallback> _EmptyChain = Array.Empty<ContextRoleFallback>();

	[CanBeNull]
	public string Primary { get; }

	[NotNull]
	public IReadOnlyList<ContextRoleFallback> Chain { get; }

	[CanBeNull]
	public string Note { get; }

	public static ContextRoleHint Empty => new ContextRoleHint(null, _EmptyChain);

	public bool HasContent
	{
		get
		{
			if (string.IsNullOrEmpty(Primary) && (Chain == null || Chain.Count <= 0))
			{
				return !string.IsNullOrEmpty(Note);
			}
			return true;
		}
	}

	public ContextRoleHint([CanBeNull] string primary, [CanBeNull] IReadOnlyList<ContextRoleFallback> chain = null, [CanBeNull] string note = null)
	{
		Primary = primary;
		Chain = chain ?? _EmptyChain;
		Note = note;
	}
}
