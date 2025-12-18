using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public class AbilitySingleTargetRange : AbilityRange, IShowAoEAffectedUIHandler, ISubscriber, IAbilityRangeManualUIHandler
{
	private Vector3 _cachedCasterPosition;

	private Vector3 _cachedTargetPosition;

	private readonly List<AbilityTargetUIData> m_OldAbilityTargets = new List<AbilityTargetUIData>();

	private readonly List<AbilityTargetUIData> m_AbilityTargets = new List<AbilityTargetUIData>();

	private int MinRangeCells => Ability.MinRangeCells;

	private int MaxRangeCells => Ability.RangeCells;

	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return Ability.GetPatternSettings() == null;
		}
		return false;
	}

	protected override void SetRangeToWorldPosition(Vector3 castPosition, TargetWrapper targetWrapper)
	{
		PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
		TargetWrapper targetWrapper2 = targetWrapper ?? Game.Instance.Controllers.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability, castPosition);
		Vector3 target = ((targetWrapper2 != null) ? targetWrapper2.Point : Game.Instance.Controllers.ClickEventsController.WorldPosition);
		Vector3 gridAdjustedPosition = AoEPatternHelper.GetGridAdjustedPosition(castPosition);
		Vector3 actualCastPosition = AoEPatternHelper.GetActualCastPosition(Ability.Caster, gridAdjustedPosition, target, MinRangeCells, MaxRangeCells);
		float sqrMagnitude = (_cachedCasterPosition - gridAdjustedPosition).sqrMagnitude;
		float sqrMagnitude2 = (_cachedTargetPosition - actualCastPosition).sqrMagnitude;
		if ((double)sqrMagnitude > 1E-05 || (double)sqrMagnitude2 > 1E-05)
		{
			_cachedCasterPosition = gridAdjustedPosition;
			_cachedTargetPosition = actualCastPosition;
			Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
			OrientedPatternData haloPattern = default(OrientedPatternData);
			bool ignoreRangesByDefault = false;
			OrientedPatternData patternData;
			using (ProfileScope.New("GetOrientedPattern"))
			{
				patternData = GetPatternData(gridAdjustedPosition, currentUnitDirection, targetWrapper2, out ignoreRangesByDefault);
			}
			NodeList nodes = Ability.Caster.GetOccupiedNodes(Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Ability.Caster));
			if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
			{
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
				abilityAreaHudInfo.pattern = patternData;
				abilityAreaHudInfo.haloPattern = haloPattern;
				abilityAreaHudInfo.casterRect = result;
				abilityAreaHudInfo.minRange = MinRangeCells;
				abilityAreaHudInfo.maxRange = MaxRangeCells;
				abilityAreaHudInfo.ignoreRangesByDefault = ignoreRangesByDefault;
				abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = false;
				abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
				CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
			}
			UnitPredictionManager.Instance.Or(null)?.SetAbilityArea(gridAdjustedPosition, actualCastPosition, patternData);
			m_OldAbilityTargets.AddRange(m_AbilityTargets);
			m_AbilityTargets.Clear();
			if (targetWrapper2 != null)
			{
				Ability.GatherAffectedTargetsData(patternData, castPosition, targetWrapper2, in m_AbilityTargets);
			}
			PushTargetUIDataEvents();
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

	private OrientedPatternData GetPatternData(Vector3 casterPosition, Vector3 casterDirection, TargetWrapper target, out bool ignoreRangesByDefault)
	{
		ignoreRangesByDefault = false;
		if (Ability.IsSingleTarget && Ability.IsRanged)
		{
			List<GridNodeBase> singleShotAffectedNodes = Ability.GetSingleShotAffectedNodes(target);
			return new OrientedPatternData(singleShotAffectedNodes, singleShotAffectedNodes.FirstOrDefault());
		}
		if (Ability.IsChainLightning())
		{
			HashSet<GridNodeBase> chainLightingTargets = Ability.GetChainLightingTargets(target);
			return new OrientedPatternData(chainLightingTargets, chainLightingTargets.FirstOrDefault());
		}
		PartAbilityPredictionForAreaEffect partAbilityPredictionForAreaEffect = Ability.TryGetPatternDataFromAreaEffect();
		if (partAbilityPredictionForAreaEffect != null)
		{
			ignoreRangesByDefault = true;
			return partAbilityPredictionForAreaEffect.GetAreaEffectPatternNotFromPatternCenter(Ability, target ?? ((TargetWrapper)_cachedTargetPosition)) ?? OrientedPatternData.Empty;
		}
		return OrientedPatternData.Empty;
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		_cachedCasterPosition = Vector3.zero;
		_cachedTargetPosition = Vector3.zero;
		CombatHUDRenderer.Instance.RemoveAbilityAreaHUD();
	}

	public void HandleSetRangeToCasterPositionManual(AbilityData ability, TargetWrapper targetWrapper)
	{
		if (ability.IsSingleTarget)
		{
			SetRangeToCasterPosition(targetWrapper);
		}
	}
}
