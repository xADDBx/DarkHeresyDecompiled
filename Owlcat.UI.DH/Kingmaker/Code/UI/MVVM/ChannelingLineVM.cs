using System;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Owlcat.UI;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLineVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsObstructed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsDying = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Vector3> m_StartPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<Vector3> m_EndPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly MechanicEntityUIState m_MechanicEntityUIState;

	private readonly MechanicEntity m_Target;

	private AbilityData m_CurrentAbility;

	private readonly Action DisposeAction;

	private readonly Func<Vector3?> m_HologramPositionGetter;

	public readonly MechanicEntityUIState UnitState;

	public Vector3 StartObjectOffset = Vector3.zero;

	public Vector3 EndObjectOffset = Vector3.zero;

	public bool IsHologramLine => m_HologramPositionGetter != null;

	public bool HasSteadyConcentration { get; }

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<bool> IsObstructed => m_IsObstructed;

	public ReadOnlyReactiveProperty<bool> IsDying => m_IsDying;

	public ReadOnlyReactiveProperty<Vector3> StartPos => m_StartPos;

	public ReadOnlyReactiveProperty<Vector3> EndPos => m_EndPos;

	public ChannelingLineVM(MechanicEntityUIState unitState, Action disposeAction)
		: this(unitState, null, disposeAction)
	{
	}

	public ChannelingLineVM(MechanicEntityUIState unitState, Func<Vector3?> hologramPositionGetter, Action disposeAction)
	{
		DisposeAction = disposeAction;
		UnitState = unitState;
		m_HologramPositionGetter = hologramPositionGetter;
		FeatureCountableFlag featureCountableFlag = unitState.Channeling.CurrentValue?.Ability?.Caster?.Features?.SteadyConcentration;
		HasSteadyConcentration = featureCountableFlag != null && (bool)featureCountableFlag;
		OwlcatR3UnitExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			Update();
		}).AddTo(this);
	}

	private void Update()
	{
		if (m_IsDying.CurrentValue)
		{
			return;
		}
		IUIChanneling currentValue = UnitState.Channeling.CurrentValue;
		if (currentValue == null || currentValue.Owner == null || currentValue.Target == null)
		{
			m_IsDying.Value = true;
			return;
		}
		Vector3 vector2;
		bool value;
		if (m_HologramPositionGetter != null)
		{
			Vector3? vector = m_HologramPositionGetter();
			if (!vector.HasValue)
			{
				return;
			}
			vector2 = vector.Value;
			value = !ComputeReachableAt(currentValue, vector2);
		}
		else
		{
			vector2 = currentValue.Target.Point;
			value = !currentValue.IsActive;
		}
		m_IsObstructed.Value = value;
		m_StartPos.Value = currentValue.Owner.View.transform.position;
		m_EndPos.Value = vector2;
		m_IsVisible.Value = true;
	}

	private static bool ComputeReachableAt(IUIChanneling channeling, Vector3 targetPoint)
	{
		AbilityData ability = channeling.Ability;
		MechanicEntity owner = channeling.Owner;
		MechanicEntity mechanicEntity = channeling.Target?.Entity;
		if (ability == null || owner == null)
		{
			return false;
		}
		if (mechanicEntity == ability.Caster)
		{
			return false;
		}
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(mechanicEntity?.Size ?? Size.Medium);
		IntRect rectForSize2 = SizePathfindingHelper.GetRectForSize(ability.Caster.Size);
		if (WarhammerGeometryUtils.DistanceToInCells(targetPoint, rectForSize, owner.Position, rectForSize2) > ability.RangeCells)
		{
			return false;
		}
		if (!ability.NeedLoS)
		{
			return true;
		}
		return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(targetPoint, rectForSize, owner.Position, rectForSize2) != LosCalculations.CoverType.LosBlocker;
	}

	public void NotifyDeathAnimationComplete()
	{
		m_IsVisible.Value = false;
		DisposeAction();
	}
}
