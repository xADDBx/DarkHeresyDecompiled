using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/AbilityCasterStatGreaterOrEqual10")]
[TypeId("189e9c9479704f88818afc13f72e4262")]
public class AbilityCasterStatGreaterOrEqual10 : BlueprintComponent, IAbilityCasterRestriction
{
	public StatType Stat;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		return (int)caster.Actor.GetStat(Stat, null, default(StatContext), "IsCasterRestrictionPassed") >= 10;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		string text = LocalizedTexts.Instance.Stats.GetText(Stat);
		return string.Format(ConfigRoot.Instance.LocalizedTexts.Reasons.NotEnoughStat, text);
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		CasterRestrictionsStrings casterRestrictionsStrings = ConfigRoot.Instance.LocalizedTexts.CasterRestrictionsStrings;
		yield return casterRestrictionsStrings.GetStatRestrictionText(Stat, 10);
	}
}
