using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("2aa503c54af44bd5b74037ea4f833dfb")]
public class RewardChangeStatEfficiency : Reward
{
	[SerializeField]
	private int m_EfficiencyModifier;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	public int EfficiencyModifier => m_EfficiencyModifier;

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	public BlueprintColony Colony => m_Colony?.Get();
}
