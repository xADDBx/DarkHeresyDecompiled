using System;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipPointBlockVM : ViewModel, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler
{
	public readonly MechanicEntityUIState MechanicEntityUIState;

	private IDisposable m_UpdateDispatcher;

	private readonly ReactiveProperty<bool> m_NeedToShow = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_MovePoints = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_ActionPoints = new ReactiveProperty<float>(0f);

	private MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	private MechanicEntityUIWrapper MechanicEntityUIWrapper => MechanicEntityUIState.MechanicEntity;

	public ReadOnlyReactiveProperty<bool> NeedToShow => m_NeedToShow;

	public ReadOnlyReactiveProperty<float> MovePoints => m_MovePoints;

	public ReadOnlyReactiveProperty<float> ActionPoints => m_ActionPoints;

	public OvertipPointBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		if (Unit.IsInPlayerParty)
		{
			EventBus.Subscribe(this).AddTo(this);
			if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				OnNewUnitTurnCheck(Game.Instance.Controllers.TurnController.CurrentUnit);
			}
		}
	}

	protected override void OnDispose()
	{
		m_UpdateDispatcher?.Dispose();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		OnNewUnitTurnCheck(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		OnNewUnitTurnCheck(EventInvokerExtensions.MechanicEntity);
	}

	private void OnNewUnitTurnCheck(MechanicEntity unit)
	{
		m_UpdateDispatcher?.Dispose();
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			m_NeedToShow.Value = false;
			return;
		}
		if (Unit == unit)
		{
			UpdateProperties();
			m_UpdateDispatcher = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
			{
				UpdateProperties();
			});
		}
		m_NeedToShow.Value = Unit == unit;
	}

	private void UpdateProperties()
	{
		if (MechanicEntityUIWrapper.CombatState != null)
		{
			m_MovePoints.Value = MechanicEntityUIWrapper.CombatState.MovementPoints;
			m_ActionPoints.Value = MechanicEntityUIWrapper.CombatState.ActionPoints;
		}
	}
}
