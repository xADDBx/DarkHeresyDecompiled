using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartCoopProgressVM : ViewModel, INetEvents, ISubscriber
{
	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_CurrentProgress = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_TotalProgress = new ReactiveProperty<int>(0);

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public ReadOnlyReactiveProperty<int> CurrentProgress => m_CurrentProgress;

	public ReadOnlyReactiveProperty<int> TotalProgress => m_TotalProgress;

	public CombatStartCoopProgressVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		if (UtilityNet.InLobbyAndPlayingOrLoading)
		{
			m_IsActive.Value = true;
			RefreshStartBattleProgress();
			Observable.EveryUpdate().Subscribe(RefreshStartBattleProgress).AddTo(this);
		}
	}

	private void RefreshStartBattleProgress()
	{
		if (Game.Instance.Controllers.TurnController.GetStartBattleProgress(out var current, out var target, out var _))
		{
			m_CurrentProgress.Value = current;
			m_TotalProgress.Value = target;
		}
	}

	void INetEvents.HandleTransferProgressChanged(bool value)
	{
	}

	void INetEvents.HandleNetGameStateChanged(NetGame.State state)
	{
		m_IsActive.Value = PhotonManager.Lobby.IsActive;
	}

	void INetEvents.HandleNLoadingScreenClosed()
	{
	}
}
