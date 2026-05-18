using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuSettingsButtonVM : IngameMenuBaseVM, INetRoleSetHandler, ISubscriber, INetEvents, IPauseHandler, IAreaHandler, IPartyCombatHandler
{
	private readonly ReactiveProperty<bool> m_PlayerHaveRoles = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NetFirstLoadState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowPauseButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsPause = new ReactiveProperty<bool>();

	private bool IsNotServiceWindow => RootUIContext.Instance.CurrentServiceWindow == ServiceWindowsType.None;

	public ReadOnlyReactiveProperty<bool> PlayerHaveRoles => m_PlayerHaveRoles;

	public ReadOnlyReactiveProperty<bool> NetFirstLoadState => m_NetFirstLoadState;

	public ReadOnlyReactiveProperty<bool> ShowPauseButton => m_ShowPauseButton;

	public ReadOnlyReactiveProperty<bool> IsPause => m_IsPause;

	public IngameMenuSettingsButtonVM([CanBeNull] ReadOnlyReactiveProperty<bool> isForceHidden)
		: base(isForceHidden)
	{
		UpdateShowPauseState();
		UpdateIsPauseState();
		HandleRoleSet(string.Empty);
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	protected override void OnDispose()
	{
	}

	public void OpenEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void Pause()
	{
		Game.Instance.PauseBind();
	}

	public void OpenNetRoles()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	protected override void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			m_ShouldShow.Value = base.IsAppropriateGameMode && IsNotServiceWindow && base.ShouldShow.CurrentValue;
		}
	}

	public void HandleRoleSet(string entityId)
	{
		m_PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	public void HandleTransferProgressChanged(bool value)
	{
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandleNLoadingScreenClosed()
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void OnPauseToggled()
	{
		UpdateIsPauseState();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		UpdateShowPauseState();
		UpdateIsPauseState();
	}

	private void UpdateShowPauseState()
	{
		m_ShowPauseButton.Value = !RootUIContext.Instance.GameUIState.IsInCombat.Value;
	}

	private void UpdateIsPauseState()
	{
		m_IsPause.Value = Game.Instance.IsPaused && !Game.Instance.Controllers.PauseController.IsPausedByPlayers;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateShowPauseState();
	}
}
