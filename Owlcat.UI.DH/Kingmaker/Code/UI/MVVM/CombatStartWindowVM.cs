using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartWindowVM : ViewModel, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber
{
	public readonly bool CanDeploy;

	private readonly ReactiveProperty<bool> m_CanStartCombat = new ReactiveProperty<bool>();

	public readonly CombatStartCoopProgressVM CoopProgressVM;

	private readonly ReactiveProperty<PartyVM> m_PartyVM;

	private readonly ReactiveProperty<string> m_CannotStartCombatReason = new ReactiveProperty<string>();

	private readonly Action m_StartBattle;

	public ReadOnlyReactiveProperty<bool> CanStartCombat => m_CanStartCombat;

	public ReadOnlyReactiveProperty<PartyVM> PartyVM => m_PartyVM;

	public ReadOnlyReactiveProperty<string> CannotStartCombatReason => m_CannotStartCombatReason;

	public CombatStartWindowVM(Action startBattle, ReactiveProperty<PartyVM> partyVM, bool canDeploy)
	{
		CoopProgressVM = new CombatStartCoopProgressVM().AddTo(this);
		m_PartyVM = partyVM;
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateCanStartCombat();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		m_StartBattle = startBattle;
		CanDeploy = canDeploy;
		UpdateCanStartCombat();
		m_PartyVM.CurrentValue.SelectFirstCharacter();
	}

	public void StartBattle()
	{
		if (!CanStartCombat.CurrentValue)
		{
			ShowCantStartBattleWarning();
			return;
		}
		m_StartBattle?.Invoke();
		m_CanStartCombat.Value = false;
	}

	public void HandleEntityPositionChanged()
	{
		UpdateCanStartCombat();
	}

	private void UpdateCanStartCombat()
	{
		bool flag = Game.Instance.Controllers.TurnController.CanFinishDeploymentPhase();
		string value = ((!flag) ? UIStrings.Instance.TurnBasedTexts.CannotStartbattle.Text : string.Empty);
		if (Game.Instance.Controllers.TurnController.GetStartBattleProgress(out var current, out var target, out var playerGroup) && playerGroup.Contains(NetworkingManager.LocalNetPlayer))
		{
			flag = false;
			value = string.Format(UIStrings.Instance.CommonTexts.WaitingOtherPlayer, current, target);
		}
		m_CannotStartCombatReason.Value = value;
		m_CanStartCombat.Value = flag;
	}

	private void ShowCantStartBattleWarning()
	{
		if (!string.IsNullOrEmpty(CannotStartCombatReason.CurrentValue))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(CannotStartCombatReason.CurrentValue, addToLog: false, WarningNotificationFormat.Warning);
			});
		}
	}
}
