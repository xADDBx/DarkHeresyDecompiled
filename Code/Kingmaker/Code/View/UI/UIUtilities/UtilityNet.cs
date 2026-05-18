using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityNet
{
	public static bool InLobbyAndPlaying => PhotonManager.NetGame.CurrentState == NetGame.State.Playing;

	public static bool InLobbyAndPlayingOrLoading
	{
		get
		{
			if (!InLobbyAndPlaying && PhotonManager.NetGame.CurrentState != NetGame.State.UploadSaveAndStartLoading)
			{
				return PhotonManager.NetGame.CurrentState == NetGame.State.DownloadSaveAndLoading;
			}
			return true;
		}
	}

	private static bool LocalTest([CanBeNull] this Entity unit, out bool isMine)
	{
		isMine = false;
		return false;
	}

	public static bool IsControlMainCharacter()
	{
		return Game.Instance.Player.MainCharacterEntity.IsMyNetRole();
	}

	public static bool CanEditCareer([CanBeNull] this MechanicEntity entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (Game.Instance.RootUIContext.IsCharGenShown)
		{
			return IsControlMainCharacter();
		}
		return false;
	}

	public static bool IsControlMainCharacterWithWarning(bool needSignalHowToPing = false)
	{
		if (IsControlMainCharacter())
		{
			return true;
		}
		UINetLobbyTexts netLobbyTexts = UIStrings.Instance.NetLobbyTexts;
		string text = netLobbyTexts.WarningPlayerIsNotControlMainCharacter;
		if (needSignalHowToPing)
		{
			text = text + Environment.NewLine + (Game.Instance.IsControllerMouse ? netLobbyTexts.HowToPingCoopLabelPc : netLobbyTexts.HowToPingCoopLabelConsole);
		}
		UtilityMessageBox.SendWarning(text);
		return false;
	}

	public static bool IsAlsoControlMainCharacter([CanBeNull] this MechanicEntity entry)
	{
		if (!InLobbyAndPlaying)
		{
			return true;
		}
		PhotonActorNumber player = entry.GetPlayer();
		if (!player.IsValid)
		{
			return false;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		NetPlayer player2 = player.ToNetPlayer(NetPlayer.Empty);
		return mainCharacterEntity.IsDirectlyControllable(player2);
	}

	public static bool IsAlsoControlMainCharacterWithWarning([CanBeNull] this MechanicEntity entry)
	{
		if (entry.IsAlsoControlMainCharacter())
		{
			return true;
		}
		if (entry.IsDirectlyControllable())
		{
			UtilityMessageBox.SendWarning(UIStrings.Instance.NetLobbyTexts.WarningPlayerIsNotControlMainCharacter);
		}
		return false;
	}

	public static bool IsDirectlyControllable([CanBeNull] this MechanicEntity entry)
	{
		return entry.IsDirectlyControllable(NetworkingManager.LocalNetPlayer);
	}

	public static bool IsDirectlyControllable([CanBeNull] this MechanicEntity entry, NetPlayer player)
	{
		if (entry != null && entry.IsDirectlyControllable)
		{
			return entry.IsNetRoleInternal(player);
		}
		return false;
	}

	public static bool IsMyNetRole([CanBeNull] this MechanicEntity entry)
	{
		return entry.IsDirectlyControllable();
	}

	public static bool CanBeControlled([CanBeNull] this MechanicEntity entry)
	{
		return entry.CanBeControlled(NetworkingManager.LocalNetPlayer);
	}

	public static bool CanBeControlled([CanBeNull] this MechanicEntity entry, NetPlayer player)
	{
		return entry.IsNetRoleInternal(player);
	}

	private static bool IsNetRoleInternal([CanBeNull] this MechanicEntity entity, NetPlayer player)
	{
		if (entity.LocalTest(out var isMine))
		{
			return isMine;
		}
		if (Game.Instance.IsSpaceCombat)
		{
			return entity != null;
		}
		if (InLobbyAndPlayingOrLoading)
		{
			if (entity != null)
			{
				return Game.Instance.CoopData.PlayerRole.Can(entity, player);
			}
			return false;
		}
		return true;
	}

	public static PhotonActorNumber GetPlayer(this Entity entity)
	{
		if (entity == null)
		{
			return PhotonActorNumber.Invalid;
		}
		if (!InLobbyAndPlayingOrLoading)
		{
			return PhotonActorNumber.Invalid;
		}
		foreach (PlayerInfo activePlayer in PhotonManager.Instance.ActivePlayers)
		{
			PhotonActorNumber player = activePlayer.Player;
			NetPlayer player2 = player.ToNetPlayer(NetPlayer.Empty);
			if (Game.Instance.CoopData.PlayerRole.Can(entity, player2))
			{
				return player;
			}
		}
		return PhotonActorNumber.Invalid;
	}

	public static Sprite GetPlayerIcon(this PhotonActorNumber player)
	{
		PhotonManager.Player.GetIconLarge(player, out var value);
		if (!(value != null))
		{
			return ConfigRoot.Instance.UIConfig.DefaultNetAvatar;
		}
		return Sprite.Create(value, new Rect(0f, 0f, value.width, value.height), new Vector2(0.5f, 0.5f));
	}

	public static string GetUnitNameWithPlayer(this MechanicEntity entity)
	{
		if (!(entity?.GetFactionOptional()?.IsPlayer).GetValueOrDefault())
		{
			return entity.Name;
		}
		PhotonActorNumber player = entity.GetPlayer();
		if (!player.IsValid || entity.IsMyNetRole() || !PhotonManager.Player.GetNickName(player, out var nickName))
		{
			return entity.Name;
		}
		return "[" + nickName + "] " + entity.Name;
	}

	public static void ShowBlockedPlayerWarning(string playerName)
	{
		UtilityMessageBox.SendWarning(string.Format(UIStrings.Instance.NetLobbyTexts.BlockedPlayerInLobby, playerName));
	}

	public static void ShowCantJoinByPrivacySettingsWarning()
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.CantJoinLobbyDuePrivacySettings, DialogMessageBoxType.Message, null);
	}

	public static void HandlePhotonDisconnectedError(DisconnectCause cause, bool allowReconnect)
	{
		string text = UIStrings.Instance.NetLobbyErrorsTexts.PhotonDisconnectedErrorMessage.Text + " " + ReasonStrings.Instance.GetDisconnectCause(cause);
		if (allowReconnect)
		{
			ShowReconnectDialog(text);
			return;
		}
		UtilityMessageBox.ShowMessageBox(text, DialogMessageBoxType.Message, delegate
		{
			CloseLobby();
		});
	}

	public static void ShowReconnectDialog(string message)
	{
		UtilityMessageBox.ShowMessageBox(message, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton btn)
		{
			if (btn == DialogMessageBoxButton.Yes)
			{
				PhotonManager.NetGame.StartNetGameIfNeeded();
			}
			else
			{
				CloseLobby();
			}
		}, null, UIStrings.Instance.NetLobbyTexts.Reconnect, UIStrings.Instance.CommonTexts.Cancel);
	}

	private static void CloseLobby()
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyClose();
		});
	}
}
