using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("db62e95e1c4b42dea319f4a5c63493a5")]
public sealed class CheckBuffGroup : BoolPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BpRef<BlueprintAbilityGroup>[] Groups = new BpRef<BlueprintAbilityGroup>[0];

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Buff group is " + string.Join("|", Groups.Dereference().NotNull());
	}

	protected override bool GetBaseValue()
	{
		BlueprintScriptableObject blueprint = EvalContext.Current.Blueprint;
		BlueprintBuff buff = blueprint as BlueprintBuff;
		if (buff != null)
		{
			return Groups.HasItem((BpRef<BlueprintAbilityGroup> i) => buff.AbilityGroups.HasReference((BlueprintAbilityGroup?)i));
		}
		return false;
	}
}
