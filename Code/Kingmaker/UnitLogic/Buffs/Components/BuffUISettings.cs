using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("UI/BuffUISettings")]
[TypeId("1afd6596f0644d7c8267ac7a6ae2a3ec")]
public class BuffUISettings : BlueprintComponent
{
	[SerializeField]
	private BuffUIFlags UIFlags;

	public bool OverrideBuffGroupType;

	[ShowIf("OverrideBuffGroupType")]
	public List<BuffUIGroup> Groups = new List<BuffUIGroup>();

	public bool ShowInSpecial;

	[ShowIf("ShowInSpecial")]
	[Tooltip("Для каких целей бафф считается важным")]
	public BuffTargetType ImportantForTargets = BuffTargetType.All;

	[ShowIf("ShowInSpecial")]
	public List<BuffSpecialSettings> SpecialSettings = new List<BuffSpecialSettings>();

	public bool BuffCategoryOverriden => UIFlags != BuffUIFlags.None;

	public BuffGroupType GetGroup(BuffTargetType targetType)
	{
		BuffGroupType result = BuffGroupType.None;
		if (!OverrideBuffGroupType)
		{
			return result;
		}
		foreach (BuffUIGroup group in Groups)
		{
			if (group.Targets.HasFlag(targetType))
			{
				result = group.Group;
				break;
			}
		}
		return result;
	}

	public bool CheckImportantBuffConditions(BuffTargetType targetType)
	{
		if (!ShowInSpecial || !ImportantForTargets.HasFlag(targetType))
		{
			return false;
		}
		foreach (BuffSpecialSettings specialSetting in SpecialSettings)
		{
			if (specialSetting.Targets.HasFlag(targetType) && !specialSetting.Conditions.Check())
			{
				return false;
			}
		}
		return true;
	}

	public bool HasFlag(BuffUIFlags flag)
	{
		return UIFlags.HasFlag(flag);
	}
}
