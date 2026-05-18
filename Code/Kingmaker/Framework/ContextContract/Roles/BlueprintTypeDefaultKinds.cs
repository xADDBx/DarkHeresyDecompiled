using System;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class BlueprintTypeDefaultKinds
{
	public static ContextEntryPointKind? For(Type blueprintType)
	{
		if (blueprintType == null)
		{
			return null;
		}
		Type type = blueprintType;
		while (type != null)
		{
			if (type == typeof(BlueprintAbility))
			{
				return ContextEntryPointKind.AbilityOnCast;
			}
			if (type == typeof(BlueprintBuff))
			{
				return ContextEntryPointKind.BuffComponentLifecycle;
			}
			if (type == typeof(BlueprintFeature))
			{
				return ContextEntryPointKind.FeatureComponentLifecycle;
			}
			type = type.BaseType;
		}
		return null;
	}
}
