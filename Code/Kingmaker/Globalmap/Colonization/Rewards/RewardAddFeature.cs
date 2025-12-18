using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("cb27cbda91354399b9c6f462df7ef378")]
public class RewardAddFeature : Reward
{
	[SerializeField]
	private bool m_AddToParty;

	[HideIf("m_AddToParty")]
	[SerializeReference]
	[SerializeField]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public bool AddToParty => m_AddToParty;

	public AbstractUnitEvaluator UnitEvaluator => m_Unit;

	public BlueprintUnitFact Fact => m_Fact?.Get();
}
