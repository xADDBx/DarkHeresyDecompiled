using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("Ability/Movement/AbilityCustomMoveToTarget")]
[TypeId("e627a85beb904bf2bd6608ecdbcbadbc")]
public class AbilityCustomMoveToTarget : AbilityCustomLogic, IAbilityTargetRestriction
{
	[SerializeField]
	private bool m_OverrideMaxSpeed;

	[SerializeField]
	[ShowIf("m_OverrideMaxSpeed")]
	private float m_MaxSpeedOverride = 10f;

	[SerializeField]
	private ActionList m_ActionsOnTargetAfterMoved;

	[SerializeField]
	private ActionList m_ActionsAfterMoved;

	public bool DisableAttacksOfOpportunity;

	[ShowIf("DisableAttacksOfOpportunity")]
	public bool DisableOnlyIfHasFact;

	[SerializeField]
	private bool m_PassThroughAllUnits;

	[KDB("Если стоит CasterMustStandInTarget, кастер должен мочь встать в клетку, иначе кастовать нельзя")]
	[HideIf("m_CasterMustStandNearTarget")]
	[SerializeField]
	private bool m_CasterMustStandInTarget;

	[KDB("Если стоит CasterMustStandNearTarget, около цели должна быть свободная клетка для кастера, иначе кастовать нельзя")]
	[HideIf("m_CasterMustStandInTarget")]
	[SerializeField]
	private bool m_CasterMustStandNearTarget;

	[KDB("Юнит должен подойти к цели как можно ближе")]
	[SerializeField]
	private bool m_CasterMustStandAsCloseAsPossible;

	public bool IgnoreObstacles;

	public bool AllowNotStraightMovement;

	[ShowIf("DisableOnlyIfHasFact")]
	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintUnitFactReference m_CheckedFact;

	public override bool IsMoveUnit => true;

	public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity caster2 = context.Caster;
		if (!(caster2 is UnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		PartUnitCommands commands = caster.GetCommandsOptional();
		if (commands == null)
		{
			PFLog.Default.Error("Commands is missing");
			yield break;
		}
		UnitMovementAgent maybeMovementAgent = caster.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			PFLog.Default.Error("Caster movement agent is missing");
			yield break;
		}
		caster.StopMoving();
		maybeMovementAgent.MaxSpeedOverride = (m_OverrideMaxSpeed ? new float?(m_MaxSpeedOverride) : null);
		caster.Features.IsCharging.Retain(context.Ability.Fact);
		TargetWrapper target = targetWrapper;
		AbilityUnrestrictedRangeForTarget component = context.Ability.Blueprint.GetComponent<AbilityUnrestrictedRangeForTarget>();
		bool restrictRange = component == null || !component.IsRangeUnrestrictedForTarget(context.Ability, targetWrapper);
		if (m_PassThroughAllUnits)
		{
			caster.Features.CanPassThroughUnits.Retain();
		}
		if (TryGetExplicitTargetNode(caster, targetWrapper, restrictRange, out var result))
		{
			target = new TargetWrapper(result.Vector3Position());
		}
		using PathDisposable<WarhammerPathPlayer> pd = PathfindingService.Instance.FindPathTB_Delayed(caster.MaybeMovementAgent, target, limitRangeByActionPoints: false, 1, this);
		WarhammerPathPlayer path = pd.Path;
		while (!path.IsDoneAndPostProcessed())
		{
			yield return null;
		}
		if (m_PassThroughAllUnits)
		{
			caster.Features.CanPassThroughUnits.Release();
		}
		if (path.error || path.vectorPath.Count < 2)
		{
			PFLog.Default.Error("Can't find path");
			yield break;
		}
		path.OverrideBlockMode(BlockMode.Ignore);
		if (!targetWrapper.IsPoint && path.vectorPath.Count > 0 && path.vectorPath.Last().GetNearestNodeXZ().TryGetFirstUnit(out var unit) && unit != null && !unit.IsDeadOrUnconscious && unit != caster)
		{
			path.vectorPath.RemoveAt(path.vectorPath.Count - 1);
		}
		UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(path), 0f);
		if (DisableAttacksOfOpportunity && (!DisableOnlyIfHasFact || caster.Facts.Contains(CheckedFact)))
		{
			unitMoveToProperParams.DisableAttackOfOpportunity.Retain();
		}
		UnitCommandHandle moveCmdHandle = commands.AddToQueueFirst(unitMoveToProperParams);
		while (!moveCmdHandle.IsFinished)
		{
			yield return null;
			if (!(moveCmdHandle.TimeSinceStart <= 5f))
			{
				moveCmdHandle.ForceFinishForTurnBased(AbstractUnitCommand.ResultType.Success);
				caster.Position = path.vectorPath.Last();
				PFLog.Default.ErrorWithReport("Move command takes too long time, force finished");
				break;
			}
		}
		if (targetWrapper.Entity is UnitEntity unitEntity)
		{
			using (EvalContext.PushContext(context, unitEntity))
			{
				m_ActionsOnTargetAfterMoved?.Run();
			}
		}
		m_ActionsAfterMoved?.Run();
		caster.CombatState.RegisterMoveCells(path.path.Count - 1);
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		if (context.Caster is UnitEntity unitEntity)
		{
			unitEntity.View.MovementAgent.MaxSpeedOverride = null;
			unitEntity.Features.IsCharging.Release(context.Ability.Fact);
		}
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability, target, casterPosition, out failReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability, target, casterPosition, out var failReason);
		return failReason;
	}

	private bool CheckTargetRestriction(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		IntRect toSize;
		if (targetWrapper.Entity == null)
		{
			toSize = default(IntRect);
			_ = Vector3.forward;
		}
		else
		{
			toSize = targetWrapper.Entity.SizeRect;
			_ = targetWrapper.Entity.Forward;
		}
		MechanicEntity caster = ability.Caster;
		int num = WarhammerGeometryUtils.DistanceToInCells(casterPosition, caster.SizeRect, targetWrapper.Point, toSize);
		if (num < ability.Blueprint.MinRange)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		AbilityUnrestrictedRangeForTarget component = ability.Blueprint.GetComponent<AbilityUnrestrictedRangeForTarget>();
		bool flag = component == null || !component.IsRangeUnrestrictedForTarget(ability, targetWrapper);
		if (num > ability.RangeCells && flag)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		if (casterPosition.GetNearestNodeXZ()?.Area != targetWrapper.NearestNode.Area)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.PathBlocked;
			return false;
		}
		if (!AllowNotStraightMovement && (ObstacleAnalyzer.TraceAlongNavmesh(casterPosition, targetWrapper.Point) - targetWrapper.Point).sqrMagnitude >= 1E-08f && !IgnoreObstacles)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
			return false;
		}
		if (AllowNotStraightMovement && (m_CasterMustStandInTarget || m_CasterMustStandNearTarget || m_CasterMustStandAsCloseAsPossible))
		{
			if (!TryGetExplicitTargetNode(caster, targetWrapper, flag, out var result))
			{
				failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.PathBlocked;
				return false;
			}
			List<GridNodeBase> pathToTarget = GetPathToTarget(caster, casterPosition.GetNearestNodeXZ(), (GridNodeBase)result);
			if (pathToTarget[pathToTarget.Count - 1] != result)
			{
				failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.PathBlocked;
				return false;
			}
		}
		failReason = null;
		return true;
	}

	private bool TryGetExplicitTargetNode(MechanicEntity caster, TargetWrapper targetWrapper, bool restrictRange, out GraphNode result)
	{
		result = null;
		IntRect rect = ((targetWrapper.Entity != null) ? targetWrapper.Entity.SizeRect : default(IntRect));
		GridNode currentUnwalkableNode = caster.CurrentUnwalkableNode;
		if (m_CasterMustStandInTarget)
		{
			GridNodeBase nearestNodeXZUnwalkable = targetWrapper.Point.GetNearestNodeXZUnwalkable();
			if (caster.CanStandHere(nearestNodeXZUnwalkable))
			{
				result = nearestNodeXZUnwalkable;
				return true;
			}
		}
		else if (m_CasterMustStandNearTarget)
		{
			GridNodeBase nearestNodeXZUnwalkable2 = targetWrapper.Point.GetNearestNodeXZUnwalkable();
			Vector3 vector = nearestNodeXZUnwalkable2.Vector3Position();
			Vector3 vector2 = currentUnwalkableNode.Vector3Position();
			float num = float.MaxValue;
			foreach (GridNodeBase item in GridAreaHelper.GetNodesAround(nearestNodeXZUnwalkable2, rect, caster.SizeRect))
			{
				if (caster.CanStandHere(item))
				{
					Vector3 vector3 = item.Vector3Position();
					float num2 = Vector3.SqrMagnitude(vector3 - vector2) + Vector3.SqrMagnitude(vector3 - vector);
					if (num2 < num)
					{
						num = num2;
						result = item;
					}
				}
			}
			if (result != null)
			{
				return true;
			}
		}
		else if (m_CasterMustStandAsCloseAsPossible)
		{
			List<GridNodeBase> pathToTarget = GetPathToTarget(caster, currentUnwalkableNode, targetWrapper.NearestNode, restrictRange);
			for (int num3 = pathToTarget.Count - 1; num3 >= 0; num3--)
			{
				GridNodeBase gridNodeBase = pathToTarget[num3];
				if (caster.CanStandHere(gridNodeBase) && currentUnwalkableNode != gridNodeBase)
				{
					result = gridNodeBase;
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private List<GridNodeBase> GetPathToTarget(MechanicEntity caster, GridNodeBase casterNode, GridNodeBase targetNode, bool limitRangeByActionPoints = false)
	{
		return PathfindingService.Instance.FindPathTB_Blocking(caster.MaybeMovementAgent, casterNode.Vector3Position(), targetNode.Vector3Position(), limitRangeByActionPoints, ignoreThreateningAreaCost: false, m_PassThroughAllUnits).path.OfType<GridNodeBase>().ToTempList();
	}
}
