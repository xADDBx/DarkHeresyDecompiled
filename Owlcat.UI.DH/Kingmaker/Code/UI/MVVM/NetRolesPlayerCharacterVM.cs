using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesPlayerCharacterVM : ViewModel, INetRoleSetHandler, ISubscriber
{
	private readonly ReactiveProperty<bool> m_CanUp = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_CanDown = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_PlayerRoleMe = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly bool m_CanUpValue;

	private readonly bool m_CanDownValue;

	private readonly bool m_IsEmpty;

	public ReadOnlyReactiveProperty<bool> CanUp => m_CanUp;

	public ReadOnlyReactiveProperty<bool> CanDown => m_CanDown;

	public ReadOnlyReactiveProperty<bool> PlayerRoleMe => m_PlayerRoleMe;

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public UnitReference Character { get; }

	public PhotonActorNumber PlayerOwner { get; }

	public NetRolesPlayerCharacterVM(UnitReference character, PhotonActorNumber playerOwner)
	{
		EventBus.Subscribe(this).AddTo(this);
		Character = character;
		PlayerOwner = playerOwner;
		m_IsEmpty = character == null;
		if (!m_IsEmpty && PhotonManager.Instance.IsRoomOwner)
		{
			ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
			int num = activePlayers.FindIndex((PlayerInfo info) => info.Player.Equals(playerOwner));
			m_CanUpValue = num > 0;
			m_CanDownValue = num < activePlayers.Count - 1;
		}
		if (!m_IsEmpty)
		{
			BaseUnitEntity baseUnitEntity = (BaseUnitEntity)character.Entity;
			m_Portrait.Value = baseUnitEntity.Portrait.SmallPortrait;
			UpdateMoveAbility();
		}
	}

	public void MoveRoleCharacterUp()
	{
		InternalMoveCharacter(direction: false);
	}

	public void MoveRoleCharacterDown()
	{
		InternalMoveCharacter(direction: true);
	}

	private void InternalMoveCharacter(bool direction)
	{
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		int num = activePlayers.FindIndex((PlayerInfo info) => info.Player == PlayerOwner);
		MoveCharacter(activePlayers[num + (direction ? 1 : (-1))].Player);
	}

	public void MoveCharacter(PhotonActorNumber player)
	{
		if (!player.Equals(PlayerOwner))
		{
			Game.Instance.CoopData.PlayerRole.Set(Character.Id, player.ToNetPlayer(NetPlayer.Empty), enable: true);
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (!(Character == null) && !(Character.Id != entityId))
		{
			UpdateMoveAbility();
		}
	}

	private void UpdateMoveAbility()
	{
		m_PlayerRoleMe.Value = !m_IsEmpty && Game.Instance.CoopData.PlayerRole.Can(Character.Id, PlayerOwner.ToNetPlayer(NetPlayer.Empty));
		if (PhotonManager.Instance.IsRoomOwner)
		{
			m_CanUp.Value = m_CanUpValue && PlayerRoleMe.CurrentValue;
			m_CanDown.Value = m_CanDownValue && PlayerRoleMe.CurrentValue;
		}
	}
}
