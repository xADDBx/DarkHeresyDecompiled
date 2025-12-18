using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[ComponentName("Predicates/Target has stat")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("d031c0cbc9462c947a066310de0283e6")]
public class AbilityTargetStatCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public StatType Stat;

	public int GreaterThan;

	public bool Inverted;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return false;
		}
		ModifiableValue statOptional = entity.GetStatOptional(Stat);
		if (statOptional == null)
		{
			return false;
		}
		if (statOptional.BaseValue > GreaterThan)
		{
			return !Inverted;
		}
		return Inverted;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (Inverted ? ConfigRoot.Instance.LocalizedTexts.Reasons.TargetStatConditionLowerOrEqual : ConfigRoot.Instance.LocalizedTexts.Reasons.TargetStatCondition).ToString(delegate
		{
			GameLogContext.Text = LocalizedTexts.Instance.Stats.GetText(Stat);
			GameLogContext.Description = GreaterThan.ToString();
		});
	}
}
