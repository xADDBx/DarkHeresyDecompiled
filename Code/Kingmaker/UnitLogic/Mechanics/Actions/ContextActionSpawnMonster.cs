using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("c7a3b2de9c37f154797b063a5730e307")]
public class ContextActionSpawnMonster : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public ActionList AfterSpawn;

	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public bool UseCombatCRForUnit;

	[SerializeField]
	[ShowIf("UseCombatCRForUnit")]
	public BlueprintReference<BlueprintEncounter> m_Encounter;

	public ContextDurationValue DurationValue;

	public ContextValue CountValue;

	public ContextValue LevelValue = new ContextValue();

	public bool DoNotLinkToCaster;

	public bool IsDirectlyControllable;

	public bool SpawnOnPoint;

	[SerializeField]
	[SerializeReference]
	[ShowIf("SpawnOnPoint")]
	public PositionEvaluator TargetPoint;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetCaption()
	{
		return $"Summon {Blueprint?.NameSafe()} x {CountValue} for {DurationValue}";
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.Caster;
		if (caster == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		Rounds duration = DurationValue.Calculate(base.Context);
		int num = CountValue.Calculate(base.Context);
		int level = LevelValue.Calculate(base.Context);
		Vector3 vector = (SpawnOnPoint ? TargetPoint.GetValue() : base.Target.Point);
		bool flag = false;
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Blueprint.Size);
		WarhammerSingleNodeBlocker exceptBlocker = ((caster is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.View.MovementAgent.Blocker : null);
		GridNode nearestNodeXZUnwalkable = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(vector);
		if (nearestNodeXZUnwalkable != null && WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, nearestNodeXZUnwalkable, exceptBlocker))
		{
			vector = nearestNodeXZUnwalkable.Vector3Position();
			flag = true;
		}
		else
		{
			foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable, rectForSize, 2))
			{
				if (WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, item, exceptBlocker))
				{
					vector = item.Vector3Position();
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		UnitEntityView unitEntityView = Blueprint.Prefab.Load();
		float radius = ((unitEntityView != null) ? unitEntityView.Corpulence : 0.5f);
		FreePlaceSelector.PlaceSpawnPlaces(num, radius, vector);
		BlueprintEncounter blueprintEncounter = (UseCombatCRForUnit ? m_Encounter.Blueprint : null);
		using ((blueprintEncounter != null) ? ContextData<BaseUnitEntity.EncounterData>.Request().Setup(blueprintEncounter) : null)
		{
			for (int i = 0; i < num; i++)
			{
				vector = FreePlaceSelector.GetRelaxedPosition(i, projectOnGround: true);
				RulePerformSummonUnit rule = new RulePerformSummonUnit(caster, Blueprint, vector, duration, level)
				{
					Context = base.Context,
					DoNotLinkToCaster = DoNotLinkToCaster
				};
				BaseUnitEntity summonedUnit = base.Context.TriggerRule(rule).SummonedUnit;
				AbilityExecutionContext abilityContext = base.AbilityContext;
				if (abilityContext != null && abilityContext.ExecutionFromPsychicPhenomena)
				{
					summonedUnit.MarkSpawnFromPsychicPhenomena();
				}
				if (SummonPool != null)
				{
					Game.Instance.SummonPools.Register(SummonPool, summonedUnit);
				}
				BlueprintEncounter blueprintEncounter2 = base.Context.Caster?.GetOptional<PartEncounter>()?.Blueprint ?? ActiveEncounter.Current?.Blueprint;
				if (blueprintEncounter2 != null)
				{
					summonedUnit.GetOrCreate<PartEncounter>().Join(blueprintEncounter2);
				}
				using (base.Context.PushTarget(summonedUnit))
				{
					AfterSpawn.Run();
				}
			}
		}
	}
}
