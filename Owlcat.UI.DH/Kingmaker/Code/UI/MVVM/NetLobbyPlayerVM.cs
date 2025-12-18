using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyPlayerVM : ViewModel, INetDLCsHandler, ISubscriber, INetLobbyPlayersHandler
{
	private readonly ReactiveProperty<bool> m_IsMe = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsMeHost = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsEmpty = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_IsPlaying = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_UserId = new ReactiveProperty<string>();

	private readonly ReactiveProperty<PhotonActorNumber> m_UserNumber = new ReactiveProperty<PhotonActorNumber>();

	private readonly ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> m_DifferentPlatformInviteVM;

	private readonly ReactiveProperty<bool> m_EpicGamesAuthorized = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_PlayerDLcStringList = new ReactiveProperty<string>();

	public readonly GamerTagAndNameVM GamerTagAndNameVM;

	private readonly ReactiveProperty<string> m_PlayersDifferentDlcs = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<bool> IsMe => m_IsMe;

	public ReadOnlyReactiveProperty<bool> IsMeHost => m_IsMeHost;

	public ReadOnlyReactiveProperty<bool> IsEmpty => m_IsEmpty;

	public ReadOnlyReactiveProperty<bool> IsPlaying => m_IsPlaying;

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<string> UserId => m_UserId;

	public ReadOnlyReactiveProperty<bool> EpicGamesAuthorized => m_EpicGamesAuthorized;

	public ReadOnlyReactiveProperty<string> PlayerDLcStringList => m_PlayerDLcStringList;

	public ReadOnlyReactiveProperty<string> PlayersDifferentDlcs => m_PlayersDifferentDlcs;

	public NetLobbyPlayerVM(ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> differentPlatformInviteVM, ReactiveProperty<bool> epicGamesAuthorized)
	{
		m_DifferentPlatformInviteVM = differentPlatformInviteVM;
		m_EpicGamesAuthorized = epicGamesAuthorized;
		GamerTagAndNameVM = new GamerTagAndNameVM(m_UserId, m_UserNumber, m_Name).AddTo(this);
		ClearPlayer();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected NetLobbyPlayerVM()
	{
		GamerTagAndNameVM = new GamerTagAndNameVM(m_UserId, m_UserNumber, m_Name).AddTo(this);
		ClearPlayer();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void ClearPlayer()
	{
		m_IsEmpty.Value = true;
		m_PlayerDLcStringList.Value = string.Empty;
		m_PlayerDLcStringList.ForceNotify();
		m_IsMe.Value = false;
		m_IsMeHost.Value = PhotonManager.Instance != null && PhotonManager.Instance.IsRoomOwner;
		m_IsPlaying.Value = PhotonManager.Instance != null && PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		m_IsActive.Value = true;
		m_UserId.Value = null;
		m_UserNumber.Value = PhotonActorNumber.Invalid;
		m_Name.Value = string.Empty;
		m_Portrait.Value = null;
		m_PlayerDLcStringList.Value = null;
		m_PlayerDLcStringList.ForceNotify();
	}

	public virtual void SetPlayer(PhotonActorNumber player, string userId, bool isActive)
	{
		m_IsEmpty.Value = false;
		m_IsMe.Value = userId.Equals(PhotonManager.Instance.LocalPlayerUserId, StringComparison.Ordinal);
		m_IsMeHost.Value = PhotonManager.Instance.IsRoomOwner;
		m_IsPlaying.Value = PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		m_UserId.Value = userId;
		m_UserNumber.Value = player;
		m_Name.Value = ((PhotonManager.Player.GetNickName(player, out var nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : string.Empty);
		PFLog.Net.Log((!string.IsNullOrWhiteSpace(nickName)) ? ("NetLobbyPlayerVM SET NICKNAME " + nickName) : "NetLobbyPlayerVM SET USER ID EMPTY STRING");
		m_Portrait.Value = player.GetPlayerIcon();
		m_IsActive.Value = isActive;
	}

	private string GetDLCsStringList(string userID)
	{
		if (string.IsNullOrEmpty(userID))
		{
			return null;
		}
		List<IBlueprintDlc> playerDlcs = GetPlayerDlcs(userID);
		if (playerDlcs == null || !playerDlcs.Any())
		{
			return null;
		}
		if (0 >= playerDlcs.Count)
		{
			return UIStrings.Instance.NetLobbyTexts.PlayerHasNoDlcs;
		}
		IEnumerable<string> values = playerDlcs.OrderBy((IBlueprintDlc dlc) => dlc.DlcType).ToList().Select(delegate(IBlueprintDlc playerDLC)
		{
			if (!(playerDLC is BlueprintDlc blueprintDlc))
			{
				return (string)null;
			}
			return string.IsNullOrEmpty(blueprintDlc.GetDlcName()) ? blueprintDlc.name : blueprintDlc.GetDlcName();
		});
		return string.Join(Environment.NewLine, values);
	}

	private string CompareHostAndPlayerDlcs()
	{
		PlayerInfo playerInfo = PhotonManager.Instance.AllPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (playerInfo.UserId == UserId.CurrentValue)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(UserId.CurrentValue) || playerInfo.UserId == null)
		{
			return null;
		}
		List<BlueprintDlc> second = GetPlayerDlcs(UserId.CurrentValue).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType2 = dlc.DlcType;
			return dlcType2 != DlcTypeEnum.CosmeticDlc && dlcType2 != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		List<BlueprintDlc> list = GetPlayerDlcs(playerInfo.UserId).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType = dlc.DlcType;
			return dlcType != DlcTypeEnum.CosmeticDlc && dlcType != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		if (!list.Any())
		{
			return null;
		}
		List<BlueprintDlc> source = list.Except(second).ToList();
		if (!source.Any())
		{
			return null;
		}
		IEnumerable<string> values = source.OrderBy((BlueprintDlc dlc) => dlc.DlcType).ToList().Select(delegate(BlueprintDlc missingDLC)
		{
			if (missingDLC == null)
			{
				return (string)null;
			}
			return string.IsNullOrEmpty(missingDLC.GetDlcName()) ? missingDLC.name : missingDLC.GetDlcName();
		});
		return string.Join(Environment.NewLine, values);
	}

	private List<IBlueprintDlc> GetPlayerDlcs(string userID)
	{
		if (PhotonManager.DLC.TryGetPlayerDLC(userID, out var playerDLCs))
		{
			return playerDLCs;
		}
		PFLog.UI.Log("[NetLobbyPlayerVM.RefreshDLCsList] DLCs for user='" + userID + "' not found!");
		return new List<IBlueprintDlc>();
	}

	public void Kick()
	{
		UtilityMessageBox.ShowMessageBox(string.Format(UIStrings.Instance.NetLobbyTexts.KickPlayerMessage, Name.CurrentValue), DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				PhotonManager.Instance.KickPlayer(m_UserNumber.Value);
			}
		});
	}

	public void Invite()
	{
		if (PlatformServices.Platform.HasSecondaryPlatform)
		{
			ShowNetLobbyDifferentPlatformsInvite();
		}
		else
		{
			InviteFromPrimaryStore();
		}
	}

	public void InviteFromPrimaryStore()
	{
		HideNetLobbyDifferentPlatformsInvite();
		if (!PlatformServices.Platform.Invite.IsSupportInviteWindow())
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.StoreOverlayIsNotAvailable, DialogMessageBoxType.Message, null);
		}
		else
		{
			PlatformServices.Platform.Invite.ShowInviteWindow();
		}
	}

	public async void InviteFromSecondaryStore()
	{
		HideNetLobbyDifferentPlatformsInvite();
		if (!PlatformServices.Platform.Invite.IsSupportInviteWindow())
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.StoreOverlayIsNotAvailable, DialogMessageBoxType.Message, null);
			return;
		}
		Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
		if (!secondaryPlatform.IsInitialized())
		{
			await PlatformServices.Platform.InitSecondary();
		}
		secondaryPlatform.Invite.ShowInviteWindow();
	}

	private void ShowNetLobbyDifferentPlatformsInvite()
	{
		if (m_DifferentPlatformInviteVM == null)
		{
			InviteFromPrimaryStore();
		}
		else
		{
			m_DifferentPlatformInviteVM.Value = new NetLobbyInvitePlayerDifferentPlatformsVM(HideNetLobbyDifferentPlatformsInvite, InviteFromPrimaryStore, InviteFromSecondaryStore);
		}
	}

	private void HideNetLobbyDifferentPlatformsInvite()
	{
		m_DifferentPlatformInviteVM.Value?.Dispose();
		m_DifferentPlatformInviteVM.Value = null;
	}

	void INetDLCsHandler.HandleDLCsListChanged()
	{
		m_PlayerDLcStringList.Value = GetDLCsStringList(UserId.CurrentValue);
		m_PlayerDLcStringList.ForceNotify();
		m_PlayersDifferentDlcs.Value = CompareHostAndPlayerDlcs();
		m_PlayersDifferentDlcs.ForceNotify();
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
		m_PlayersDifferentDlcs.Value = CompareHostAndPlayerDlcs();
		m_PlayersDifferentDlcs.ForceNotify();
	}
}
