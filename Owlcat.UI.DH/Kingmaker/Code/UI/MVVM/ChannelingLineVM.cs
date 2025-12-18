using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLineVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Vector3> m_StartPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<Vector3> m_EndPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly MechanicEntityUIState m_MechanicEntityUIState;

	private readonly MechanicEntity m_Target;

	private AbilityData m_CurrentAbility;

	private readonly Action DisposeAction;

	public readonly MechanicEntityUIState UnitState;

	public Vector3 StartObjectOffset = Vector3.zero;

	public Vector3 EndObjectOffset = Vector3.zero;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<Vector3> StartPos => m_StartPos;

	public ReadOnlyReactiveProperty<Vector3> EndPos => m_EndPos;

	public ChannelingLineVM(MechanicEntityUIState unitState, Action disposeAction)
	{
		DisposeAction = disposeAction;
		UnitState = unitState;
		OwlcatR3UnitExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			Update();
		}).AddTo(this);
	}

	private void Update()
	{
		IUIChanneling currentValue = UnitState.Channeling.CurrentValue;
		if (currentValue == null || !currentValue.IsActive || currentValue.Owner == null || (object)currentValue.Target == null)
		{
			m_IsVisible.Value = false;
			DisposeAction();
			return;
		}
		Vector3 position = UnitState.Channeling.CurrentValue.Owner.View.transform.position;
		m_StartPos.Value = position;
		m_EndPos.Value = UnitState.Channeling.CurrentValue.Target.Point;
		m_IsVisible.Value = true;
	}
}
