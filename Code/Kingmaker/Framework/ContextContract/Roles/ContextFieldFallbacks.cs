using System;
using System.Collections.Generic;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class ContextFieldFallbacks
{
	private static readonly IReadOnlyList<ContextRoleFallback> _Empty = Array.Empty<ContextRoleFallback>();

	private static readonly IReadOnlyDictionary<ContextField, IReadOnlyList<ContextRoleFallback>> _Table = new Dictionary<ContextField, IReadOnlyList<ContextRoleFallback>>
	{
		{
			ContextField.Caster,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Owner)
			}
		},
		{
			ContextField.Owner,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Caster)
			}
		},
		{
			ContextField.ClickedTarget,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Owner)
			}
		},
		{
			ContextField.Target,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.ClickedTarget)
			}
		},
		{
			ContextField.SourceAbility,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Ability)
			}
		},
		{
			ContextField.SourceCaster,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Caster)
			}
		},
		{
			ContextField.SourceBlueprint,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Blueprint)
			}
		},
		{
			ContextField.SourceFact,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Fact)
			}
		},
		{
			ContextField.SourceClickedTarget,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.ClickedTarget)
			}
		},
		{
			ContextField.SourceAbilityBlueprint,
			new ContextRoleFallback[1]
			{
				new ContextRoleFallback(ContextField.Ability, "blueprint")
			}
		}
	};

	public static IReadOnlyList<ContextRoleFallback> Chain(ContextField field)
	{
		if (!_Table.TryGetValue(field, out var value))
		{
			return _Empty;
		}
		return value;
	}
}
