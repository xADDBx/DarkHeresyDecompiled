using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("84b99720cae34d2f93405bff58fbbfb7")]
public class RewardConsumable : Reward
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	[SerializeField]
	private int m_MaxCount;

	[SerializeField]
	private int m_SegmentsToRefill;

	private BlueprintColonyReference m_Colony;

	public int MaxCount => m_MaxCount;

	public BlueprintItem Item => m_Item?.Get();

	public int SegmentsToRefill => m_SegmentsToRefill;

	public BlueprintColony Colony => m_Colony?.Get();
}
