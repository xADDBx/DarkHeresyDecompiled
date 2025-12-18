using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("f99cbe15c48f4c8d8d09f619518999fb")]
public class AbilityRequirementHasCondition : BlueprintComponent, IAbilityRestriction
{
	public bool Not;

	public UnitCondition[] Conditions;

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		return false;
	}

	public string GetAbilityRestrictionUIText()
	{
		LocalizedString obj = (Not ? ConfigRoot.Instance.LocalizedTexts.Reasons.HasForbiddenCondition : ConfigRoot.Instance.LocalizedTexts.Reasons.NoRequiredCondition);
		string conditions = string.Join(", ", Conditions.Select(UtilityText.GetConditionText));
		return obj.ToString(delegate
		{
			GameLogContext.Text = conditions;
		});
	}
}
