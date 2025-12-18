using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("025c75e262144576a8d4c9ac67917352")]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
public class RewardColonyTrait : Reward
{
	[SerializeField]
	private BlueprintColonyTrait.Reference m_Trait;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[SerializeField]
	[ShowIf("ShouldSpecifyColony")]
	private BlueprintColonyReference m_SpecificColony;

	public BlueprintColonyTrait Trait => m_Trait?.Get();

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	private bool ShouldSpecifyColony
	{
		get
		{
			if (!(base.OwnerBlueprint is BlueprintColonyProject))
			{
				return !m_ApplyToAllColonies;
			}
			return false;
		}
	}

	private BlueprintColony SpecificColony => m_SpecificColony?.Get();
}
