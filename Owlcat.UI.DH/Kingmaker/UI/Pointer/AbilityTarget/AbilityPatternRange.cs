using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.Predictions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public class AbilityPatternRange : AbilityRange, IShowAoEAffectedUIHandler, ISubscriber, IAbilityRangeManualUIHandler
{
	private readonly List<GameObject> m_CellMarkers = new List<GameObject>();

	public GameObject CellMarker;

	private GridNodeBase m_CachedCasterNode;

	private GridNodeBase m_CachedTargetNode;

	private readonly List<AbilityTargetUIData> m_OldAbilityTargets = new List<AbilityTargetUIData>();

	private readonly List<AbilityTargetUIData> m_AbilityTargets = new List<AbilityTargetUIData>();

	private int MinRangeCells => Ability.MinRangeCells;

	private int MaxRangeCells => Ability.RangeCells;

	private IAbilityAoEPatternProvider PatternProvider => Ability.GetPatternSettings();

	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return PatternProvider != null;
		}
		return false;
	}

	protected override void SetRangeToCasterPosition(TargetWrapper targetWrapper)
	{
		SetRangeToWorldPosition(GetBestCastingPosition(Ability.Caster).Vector3Position(), targetWrapper);
	}

	public GridNodeBase GetBestCastingPosition(MechanicEntity caster)
	{
		PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(caster);
		return caster.GetInnerNodeNearestToTarget(desiredPosition.GetNearestNodeXZUnwalkable(), clickEventsController.WorldPosition);
	}

	protected override void SetRangeToWorldPosition(Vector3 castPosition, TargetWrapper targetWrapper)
	{
		using (ProfileScope.New("SetRangeToWorldPosition"))
		{
			foreach (GameObject cellMarker in m_CellMarkers)
			{
				cellMarker.SetActive(value: false);
			}
			PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
			TargetWrapper targetWrapper2 = targetWrapper ?? Game.Instance.Controllers.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability, castPosition);
			Vector3 vector = ((targetWrapper2 != null) ? targetWrapper2.Point : clickEventsController.WorldPosition);
			GridNodeBase bestShootingPositionForDesiredPosition;
			using (ProfileScope.New("GetBestShootingPositionForDesiredPosition"))
			{
				bestShootingPositionForDesiredPosition = Ability.GetBestShootingPositionForDesiredPosition(vector);
			}
			GridNodeBase actualCastNode;
			using (ProfileScope.New("GetActualCastNode"))
			{
				actualCastNode = AoEPatternHelper.GetActualCastNode(Ability.Caster, bestShootingPositionForDesiredPosition, vector, MinRangeCells, MaxRangeCells);
			}
			if (m_CachedCasterNode == bestShootingPositionForDesiredPosition && m_CachedTargetNode == actualCastNode)
			{
				return;
			}
			m_CachedCasterNode = bestShootingPositionForDesiredPosition;
			m_CachedTargetNode = actualCastNode;
			actualCastNode = (GridNodeBase)(GraphNode)ObstacleAnalyzer.GetNearestNode(actualCastNode.Vector3Position());
			Size targetSizeForPattern = Ability.GetTargetSizeForPattern(targetWrapper2);
			OrientedPatternData haloPattern = default(OrientedPatternData);
			OrientedPatternData orientedPatternData;
			using (ProfileScope.New("GetOrientedPattern"))
			{
				orientedPatternData = PatternProvider.GetOrientedPattern(Ability, bestShootingPositionForDesiredPosition, actualCastNode, targetSizeForPattern);
				if (Ability.GetHaloSize(out var haloSize))
				{
					haloPattern = PatternProvider.GetOrientedHaloPattern(Ability, haloSize, bestShootingPositionForDesiredPosition, actualCastNode, targetSizeForPattern);
				}
				else if (Ability.Blueprint.IsBurst)
				{
					haloPattern = orientedPatternData;
				}
			}
			m_OldAbilityTargets.AddRange(m_AbilityTargets);
			m_AbilityTargets.Clear();
			using (ProfileScope.New("GatherAffectedTargetsData"))
			{
				Ability.GatherAffectedTargetsData(orientedPatternData, bestShootingPositionForDesiredPosition.Vector3Position(), targetWrapper2, in m_AbilityTargets);
			}
			Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Ability.Caster);
			NodeList nodes = Ability.Caster.GetOccupiedNodes(desiredPosition);
			if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
			{
				bool flag = Ability.Blueprint.ComponentsArray.Any((BlueprintComponent c) => c is AbilityAttackDelivery abilityAttackDelivery && abilityAttackDelivery.IsBurst);
				if (Ability.Blueprint.IsBurst && !Ability.CanTargetPoint)
				{
					orientedPatternData = (m_AbilityTargets.Any() ? new OrientedPatternData(orientedPatternData.Nodes.Where((GridNodeBase n) => n.GetFirstUnit() != null).ToList(), new GridNode()) : default(OrientedPatternData));
				}
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
				abilityAreaHudInfo.pattern = orientedPatternData;
				abilityAreaHudInfo.haloPattern = haloPattern;
				abilityAreaHudInfo.casterRect = result;
				abilityAreaHudInfo.minRange = MinRangeCells;
				abilityAreaHudInfo.maxRange = MaxRangeCells;
				abilityAreaHudInfo.ignoreRangesByDefault = !flag;
				abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = false;
				abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
				using (ProfileScope.New("SetAbilityAreaHUD"))
				{
					CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
				}
			}
			using (ProfileScope.New("UnitPredictionManager > SetAbilityArea"))
			{
				ObjectExtensions.Or(UnitPredictionManager.Instance, null)?.SetAbilityArea(bestShootingPositionForDesiredPosition.Vector3Position(), actualCastNode.Vector3Position(), orientedPatternData);
			}
			PushTargetUIDataEvents();
		}
	}

	protected override void ClearRange()
	{
		foreach (AbilityTargetUIData abilityTarget in m_AbilityTargets)
		{
			EventBus.RaiseEvent((IMechanicEntity)abilityTarget.Target, (Action<ICellAbilityHandler>)delegate(ICellAbilityHandler h)
			{
				h.HandleCellAbilityClear();
			}, isCheckRuntime: true);
		}
	}

	private void PushTargetUIDataEvents()
	{
		using (ProfileScope.New("PushTargetUIDataEvents"))
		{
			foreach (AbilityTargetUIData targetUIData in m_AbilityTargets)
			{
				EventBus.RaiseEvent((IMechanicEntity)targetUIData.Target, (Action<ICellAbilityHandler>)delegate(ICellAbilityHandler h)
				{
					h.HandleCellAbility(targetUIData);
				}, isCheckRuntime: true);
			}
			foreach (AbilityTargetUIData targetUIData in m_OldAbilityTargets)
			{
				if (!m_AbilityTargets.Contains((AbilityTargetUIData a) => targetUIData.Target == a.Target))
				{
					EventBus.RaiseEvent((IMechanicEntity)targetUIData.Target, (Action<ICellAbilityHandler>)delegate(ICellAbilityHandler h)
					{
						h.HandleCellAbilityClear();
					}, isCheckRuntime: true);
				}
			}
		}
		m_OldAbilityTargets.Clear();
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		foreach (GameObject cellMarker in m_CellMarkers)
		{
			cellMarker.SetActive(value: false);
		}
		m_CachedCasterNode = null;
		m_CachedTargetNode = null;
		CombatHUDRenderer.Instance.RemoveAbilityAreaHUD();
	}

	public void HandleSetRangeToCasterPositionManual(AbilityData ability, TargetWrapper targetWrapper)
	{
		if (!ability.IsSingleTarget)
		{
			SetRangeToCasterPosition(targetWrapper);
		}
	}
}
