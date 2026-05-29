using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartWindowVM : ViewModel, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber
{
	private enum StartCombatFailResult
	{
		None,
		DeploymentNotFinished,
		WaitingNetworkPlayer
	}

	private readonly ReactiveProperty<bool> m_CanStartCombat = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<PartyVM> m_PartyVM;

	private readonly ReactiveProperty<string> m_CannotStartCombatReason = new ReactiveProperty<string>();

	private readonly Action m_StartBattle;

	private StartCombatFailResult? m_CombatStartFailResult;

	public readonly bool CanDeploy;

	public readonly CombatStartCoopProgressVM CoopProgressVM;

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
		if (m_PartyVM.CurrentValue != null)
		{
			m_PartyVM.CurrentValue.SelectFirstCharacter();
		}
		else
		{
			PFLog.UI.ErrorWithReport("[WH2-51465] CombatStartWindowVM: PartyVM is null");
		}
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
		StartCombatFailResult result;
		int currentStartProgress;
		int targetStartProgress;
		bool flag = CheckCanStartCombat(out result, out currentStartProgress, out targetStartProgress);
		if (result != m_CombatStartFailResult)
		{
			StartCombatFailResult? combatStartFailResult = m_CombatStartFailResult;
			TurnController turnController = Game.Instance.Controllers.TurnController;
			PFLog.UI.Log("[WH2-51465] " + $"session={turnController.DeploymentDiagnosticsSessionId} " + "event=CombatStart.UpdateCanStartCombat " + $"previousResult={combatStartFailResult}, result={result}, " + $"canStartCombat={flag}, canDeploy={CanDeploy}, " + $"canFinishDeployment={result != StartCombatFailResult.DeploymentNotFinished}, " + $"isPreparation={turnController.IsPreparationTurn}, " + $"currentProgress={currentStartProgress}, targetProgress={targetStartProgress}, " + $"partyVM={m_PartyVM.CurrentValue != null}");
			m_CombatStartFailResult = result;
			string value = result switch
			{
				StartCombatFailResult.None => string.Empty, 
				StartCombatFailResult.WaitingNetworkPlayer => FormatNetworkMessage(currentStartProgress, targetStartProgress), 
				_ => UIStrings.Instance.TurnBasedTexts.CannotStartbattle.Text, 
			};
			m_CannotStartCombatReason.Value = value;
			m_CanStartCombat.Value = flag;
		}
		static string FormatNetworkMessage(int current, int target)
		{
			return string.Format(UIStrings.Instance.CommonTexts.WaitingOtherPlayer, current, target);
		}
	}

	private bool CheckCanStartCombat(out StartCombatFailResult result, out int currentStartProgress, out int targetStartProgress)
	{
		currentStartProgress = 0;
		targetStartProgress = 0;
		if (!Game.Instance.Controllers.TurnController.CanFinishDeploymentPhase())
		{
			result = StartCombatFailResult.DeploymentNotFinished;
			return false;
		}
		if (Game.Instance.Controllers.TurnController.GetStartBattleProgress(out currentStartProgress, out targetStartProgress, out var playerGroup) && playerGroup.Contains(NetworkingManager.LocalNetPlayer))
		{
			result = StartCombatFailResult.WaitingNetworkPlayer;
			return false;
		}
		result = StartCombatFailResult.None;
		return true;
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
