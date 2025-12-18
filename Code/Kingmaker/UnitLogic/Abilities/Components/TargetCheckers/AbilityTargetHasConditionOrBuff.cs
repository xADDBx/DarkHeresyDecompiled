using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[TypeId("2c17e0f060c2e1c46aa4cb7f9fa860df")]
public class AbilityTargetHasConditionOrBuff : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public UnitCondition Condition;

	[SerializeField]
	[FormerlySerializedAs("Buffs")]
	private BlueprintBuffReference[] m_Buffs;

	public bool BuffFromCaster;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		LocalizedString obj = (Not ? ConfigRoot.Instance.LocalizedTexts.Reasons.TargetHasNoConditionAndBuff : ConfigRoot.Instance.LocalizedTexts.Reasons.TargetHasConditionOrBuff);
		string text = string.Empty;
		if (Condition != 0)
		{
			text += UtilityText.GetConditionText(Condition);
		}
		if (Buffs.Length > 0)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += string.Join(", ", Buffs.Select((BlueprintBuff b) => b.Name));
		}
		return obj.ToString(delegate
		{
			GameLogContext.Text = text;
		});
	}
}
