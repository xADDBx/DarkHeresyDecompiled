using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/AbilityCanOnlyTargetCasterIfCasterHasBuff")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("e0bd83228ab0430db79551fb105cbd80")]
public class AbilityCanOnlyTargetCasterIfCasterHasBuff : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public LocalizedString Reason;

	public BlueprintBuff Buff => m_Buff?.Get();

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (target.Entity == null)
		{
			return false;
		}
		ability.Caster.Buffs.Contains(Buff);
		if (ability.Caster.Buffs.Contains(Buff))
		{
			return ability.Caster == target;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Reason;
	}
}
