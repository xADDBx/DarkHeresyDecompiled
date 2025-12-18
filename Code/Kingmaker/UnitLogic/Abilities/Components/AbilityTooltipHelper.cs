using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("UI/AbilityTooltipHelper")]
[TypeId("a72c1eb397ac57c47b01bf4416ced9e9")]
public class AbilityTooltipHelper : BlueprintComponent
{
	[SerializeField]
	private bool m_OverrideTargetType;

	[SerializeField]
	[ShowIf("m_OverrideTargetType")]
	private TargetType m_TargetType;

	public TargetType? TargetType
	{
		get
		{
			if (!m_OverrideTargetType)
			{
				return null;
			}
			return m_TargetType;
		}
	}
}
