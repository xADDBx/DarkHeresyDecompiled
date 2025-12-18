using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[ComponentName("Actions/StarshipVariousActions")]
[AllowMultipleComponents]
[TypeId("01f2545a552d7a44aa449aae019d7388")]
public class StarshipVariousActions : ContextAction
{
	public enum ActionType
	{
		StartingDamage,
		ApplyBuffToPlayerEnemies,
		RemoveBuffFromPlayerEnemies,
		SetFactionUnitsAsOnlyLowPriorityToOwnerBrain,
		ReduceBuffStackDuration,
		ApplyBuffToPlayerStarship,
		RunActionsOnStarshipsWithBlueprintAsTarget,
		BlockRandomWeapons
	}

	public ActionType actionType;

	[SerializeField]
	[ShowIf("DemandsBuff")]
	private BlueprintBuffReference m_Buff;

	[ShowIf("DemandsDamageStats")]
	public int hpPctDmg;

	[ShowIf("DemandsDamageStats")]
	public int[] shieldsPctDmgMin = new int[4];

	[SerializeField]
	[ShowIf("DemandsFaction")]
	private BlueprintFactionReference m_Faction;

	[SerializeField]
	[ShowIf("DemandsIntValue")]
	private int Value;

	[SerializeField]
	[ShowIf("DemandsChances")]
	private int Chances;

	[SerializeField]
	[ShowIf("DemandsStarshipBlueprint")]
	private BlueprintStarship.Reference m_StarshipBlueprint;

	[SerializeField]
	[ShowIf("DemandsActions")]
	private ActionList Actions;

	public bool DemandsBuff
	{
		get
		{
			if (actionType != ActionType.ApplyBuffToPlayerEnemies && actionType != ActionType.RemoveBuffFromPlayerEnemies && actionType != ActionType.ReduceBuffStackDuration)
			{
				return actionType == ActionType.ApplyBuffToPlayerStarship;
			}
			return true;
		}
	}

	public bool DemandsDamageStats => actionType == ActionType.StartingDamage;

	public bool DemandsFaction => actionType == ActionType.SetFactionUnitsAsOnlyLowPriorityToOwnerBrain;

	public bool DemandsIntValue
	{
		get
		{
			if (actionType != ActionType.ReduceBuffStackDuration)
			{
				return actionType == ActionType.BlockRandomWeapons;
			}
			return true;
		}
	}

	public bool DemandsChances => actionType == ActionType.ReduceBuffStackDuration;

	public bool DemandsStarshipBlueprint => actionType == ActionType.RunActionsOnStarshipsWithBlueprintAsTarget;

	public bool DemandsActions => actionType == ActionType.RunActionsOnStarshipsWithBlueprintAsTarget;

	public BlueprintFaction Faction => m_Faction?.Get();

	public BlueprintBuff Buff => m_Buff?.Get();

	public BlueprintStarship StarshipBlueprint => m_StarshipBlueprint?.Get();

	public override string GetCaption()
	{
		return $"Perform action: [{actionType}]";
	}

	protected override void RunAction()
	{
	}
}
