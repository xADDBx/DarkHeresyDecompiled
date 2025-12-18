using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[AllowMultipleComponents]
[TypeId("ce207479288128c47ad759fa5285b967")]
public class AbilityTargetHasCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public UnitCondition Condition;

	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (Not ? ConfigRoot.Instance.LocalizedTexts.Reasons.TargetHasNoCondition : ConfigRoot.Instance.LocalizedTexts.Reasons.TargetHasCondition).ToString(delegate
		{
			GameLogContext.Text = UtilityText.GetConditionText(Condition);
		});
	}
}
