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
[TypeId("2a09d956d9324196867de1714fc59e1e")]
public class RewardChangeStatContentment : Reward
{
	[SerializeField]
	private int m_ContentmentModifier;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	public int ContentmentModifier => m_ContentmentModifier;

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	public BlueprintColony Colony => m_Colony?.Get();
}
