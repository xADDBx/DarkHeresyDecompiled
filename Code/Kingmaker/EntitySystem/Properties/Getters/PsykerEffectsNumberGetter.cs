using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("a66610907168b8245aa3c103a094b00f")]
public class PsykerEffectsNumberGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public bool OnlyFromCaster;

	[ShowIf("OnlyFromCaster")]
	public PropertyTargetType Caster;

	protected override int GetBaseValue()
	{
		IEnumerable<BlueprintAbility> source = new List<BlueprintAbility>();
		foreach (Buff buff in base.CurrentEntity.Buffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbilityBlueprint != null && buff.MaybeContext.SourceAbilityBlueprint.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower) && (buff.MaybeContext.MaybeCaster == EvalContext.Current.GetEntityByType(Caster) || !OnlyFromCaster) && !source.Contains(buff.MaybeContext.SourceAbilityBlueprint))
			{
				source = source.Concat(buff.MaybeContext.SourceAbilityBlueprint);
			}
		}
		return source.Count();
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = (OnlyFromCaster ? (" from " + Caster.Colorized()) : "");
		return "Count number of buffs from Psyker powers on " + FormulaTargetScope.Current + text;
	}
}
