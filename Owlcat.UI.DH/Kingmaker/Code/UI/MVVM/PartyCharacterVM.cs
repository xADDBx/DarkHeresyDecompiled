using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.View;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PartyCharacterVM : ViewModel, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ILevelUpCompleteUIHandler, IPartyCombatHandler, ILevelUpManagerUIHandler, IPartyEncumbranceHandler, IUnitEncumbranceHandler, INetRoleSetHandler, INetStopPlayingHandler, INetLobbyPlayersHandler
{
	private readonly ReactiveProperty<bool> m_IsEnable = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSingleSelected = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsLinked = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsLevelUp = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsLevelUpCurrent = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsLevelUpInProgress = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<Sprite> m_NetAvatar = new ReactiveProperty<Sprite>(null);

	private readonly ReactiveProperty<string> m_CharacterName = new ReactiveProperty<string>(null);

	private readonly ReactiveProperty<Encumbrance> m_CharacterEncumbrance = new ReactiveProperty<Encumbrance>();

	private readonly ReactiveProperty<Encumbrance> m_PartyEncumbrance = new ReactiveProperty<Encumbrance>();

	private readonly Dictionary<BaseUnitEntity, int> m_CharacterLevel = new Dictionary<BaseUnitEntity, int>();

	private readonly ReactiveCommand<Unit> m_UpgradeLevel = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_IsNewLevel = new ReactiveCommand<Unit>();

	private readonly Action<bool> m_NextPrevAction;

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>(null);

	public readonly int Index;

	public readonly UnitHealthPartVM HealthPartVM;

	public readonly UnitPortraitPartVM PortraitPartVM;

	public readonly UnitBuffBlockVM BuffBlockVM;

	public readonly UnitBarkPartVM BarkPartVM;

	private SelectionCharacterController SelectionCharacter => Game.Instance.Controllers.SelectionCharacter;

	public ReadOnlyReactiveProperty<bool> IsEnable => m_IsEnable;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsSingleSelected => m_IsSingleSelected;

	public ReadOnlyReactiveProperty<bool> IsLinked => m_IsLinked;

	public ReadOnlyReactiveProperty<bool> IsLevelUp => m_IsLevelUp;

	public ReadOnlyReactiveProperty<bool> IsLevelUpCurrent => m_IsLevelUpCurrent;

	public ReadOnlyReactiveProperty<bool> IsLevelUpInProgress => m_IsLevelUpInProgress;

	public ReadOnlyReactiveProperty<Sprite> NetAvatar => m_NetAvatar;

	public ReadOnlyReactiveProperty<string> CharacterName => m_CharacterName;

	public ReadOnlyReactiveProperty<Encumbrance> CharacterEncumbrance => m_CharacterEncumbrance;

	public ReadOnlyReactiveProperty<Encumbrance> PartyEncumbrance => m_PartyEncumbrance;

	public bool CanSwitch
	{
		get
		{
			if (UnitEntityData != null && Game.Instance.Controllers.SelectionCharacter.ReorderEnable)
			{
				return UnitEntityData.Master == null;
			}
			return false;
		}
	}

	public BaseUnitEntity UnitEntityData => m_Unit.Value;

	public PartyCharacterVM(Action<bool> nextPrevAction, int index, ReadOnlyReactiveProperty<bool> suppressUnitSelection = null)
	{
		PartyCharacterVM partyCharacterVM = this;
		m_NextPrevAction = nextPrevAction;
		Index = index;
		EventBus.Subscribe(this).AddTo(this);
		HealthPartVM = new UnitHealthPartVM(m_Unit).AddTo(this);
		PortraitPartVM = new UnitPortraitPartVM().AddTo(this);
		BuffBlockVM = new UnitBuffBlockVM(m_Unit.Value).AddTo(this);
		BarkPartVM = new UnitBarkPartVM().AddTo(this);
		m_Unit.Subscribe(delegate(BaseUnitEntity value)
		{
			partyCharacterVM.PortraitPartVM.SetUnitData(value);
			partyCharacterVM.BuffBlockVM.SetUnitData(value);
			partyCharacterVM.BarkPartVM.SetUnitData(value);
			partyCharacterVM.UpdateLevelUpField(value);
			partyCharacterVM.UpdateEncumbranceField(value);
			partyCharacterVM.m_IsEnable.Value = value != null;
			partyCharacterVM.m_IsLinked.Value = value?.IsLink ?? false;
			partyCharacterVM.m_CharacterName.Value = value?.CharacterName;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			if (partyCharacterVM.IsEnable.CurrentValue)
			{
				bool flag = suppressUnitSelection != null && suppressUnitSelection.CurrentValue;
				partyCharacterVM.m_IsSelected.Value = !flag && partyCharacterVM.SelectionCharacter.IsSelected(partyCharacterVM.UnitEntityData);
				partyCharacterVM.m_IsSingleSelected.Value = !flag && ((partyCharacterVM.SelectionCharacter.SelectedUnitInUI.Value != null) ? (partyCharacterVM.SelectionCharacter.SelectedUnitInUI.Value == partyCharacterVM.UnitEntityData) : (partyCharacterVM.SelectionCharacter.SingleSelectedUnit.Value == partyCharacterVM.UnitEntityData));
				partyCharacterVM.m_IsLinked.Value = partyCharacterVM.UnitEntityData.IsLink;
			}
		}).AddTo(this);
		ServiceWindowsSounds.UISoundCharacter charSounds = ServiceWindowsSounds.Instance.Character;
		ObservableSubscribeExtensions.Subscribe(m_UpgradeLevel, delegate
		{
			charSounds.LevelUpgradedNotification.Play();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_IsNewLevel, delegate
		{
			charSounds.NewLevelNotification.Play();
		}).AddTo(this);
	}

	public void HandleUnitClick(bool isDoubleClick = false)
	{
		ClickUnitHandler.HandleClickControllableUnit(UnitEntityData, isDoubleClick);
	}

	public void ToggleLinkUnit()
	{
		if (SelectionManagerConsole.Instance.IsLinked(UnitEntityData))
		{
			SelectionManagerConsole.Instance.UnlinkUnit(UnitEntityData);
		}
		else
		{
			SelectionManagerConsole.Instance.LinkUnit(UnitEntityData);
		}
		UpdateLink();
	}

	public void UpdateLink()
	{
		m_IsLinked.Value = UnitEntityData?.IsLink ?? false;
	}

	public void SelectAll()
	{
		RootUIContext.Instance.SelectionManager.SelectAll();
		if (!UtilityGame.IsGlobalMap() && UnitEntityData.IsViewActive)
		{
			CameraRig.Instance.ScrollTo(UnitEntityData.Position);
			if ((bool)SettingsRoot.Controls.CameraFollowsUnit)
			{
				Game.Instance.Controllers.CameraController?.Follower?.Follow(UnitEntityData);
			}
		}
	}

	public void SetUnitData(BaseUnitEntity unitEntityData)
	{
		m_Unit.Value = unitEntityData;
		if (unitEntityData != null && !unitEntityData.IsDisposed)
		{
			m_CharacterLevel.TryAdd(unitEntityData, unitEntityData.Progression.CharacterLevel);
			if (PhotonManager.Lobby.IsActive && UnitEntityData != null)
			{
				SetNetAvatar();
			}
		}
	}

	protected override void OnDispose()
	{
		HideBark();
	}

	private void UpdateLevelUpField(BaseUnitEntity unit, bool levelUpSound = false)
	{
		if (UnitEntityData == null || unit == null || UnitEntityData != unit || UnitEntityData.IsDisposed || unit.IsDisposed)
		{
			return;
		}
		m_IsLevelUp.Value = !UIUtilityCombat.IsCombatLockActive() && UnitEntityData.Progression.CanLevelUp;
		if (levelUpSound && IsLevelUp.CurrentValue)
		{
			m_IsNewLevel.Execute(Unit.Default);
		}
		if (m_CharacterLevel.TryGetValue(unit, out var value))
		{
			if (unit.Progression.CharacterLevel > value)
			{
				m_UpgradeLevel.Execute(Unit.Default);
			}
			m_CharacterLevel[unit] = unit.Progression.CharacterLevel;
		}
	}

	private void UpdateEncumbranceField(BaseUnitEntity unit)
	{
		if (UnitEntityData != null && unit != null && UnitEntityData == unit && !UnitEntityData.IsDisposed && !unit.IsDisposed)
		{
			m_CharacterEncumbrance.Value = UnitEntityData.EncumbranceData?.Value ?? Encumbrance.Light;
			ReactiveProperty<Encumbrance> partyEncumbrance = m_PartyEncumbrance;
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			partyEncumbrance.Value = ((loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) ? Game.Instance.Player.Encumbrance : Encumbrance.Light);
		}
	}

	public void LevelUp()
	{
		if (UnitEntityData.CanBeControlled())
		{
			CharGenConfig.Create(UnitEntityData, CharGenMode.LevelUp).OpenUI();
		}
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateLevelUpField(EventInvokerExtensions.BaseUnitEntity, withSound);
	}

	public void HandleLevelUpComplete()
	{
		UpdateLevelUpField(m_Unit.Value);
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateLevelUpField(UnitEntityData);
	}

	public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateEncumbranceField(UnitEntityData);
	}

	public void ChangeUnitEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateEncumbranceField(UnitEntityData);
	}

	public void NextPrev(bool dir)
	{
		m_NextPrevAction?.Invoke(dir);
	}

	public void OnCharacterHover(bool value)
	{
		EventBus.RaiseEvent((IBaseUnitEntity)UnitEntityData, (Action<IPortraitHoverUIHandler>)delegate(IPortraitHoverUIHandler h)
		{
			h.HandlePortraitHover(value);
		}, isCheckRuntime: true);
		EventBus.RaiseEvent(delegate(IPartyCharacterHoverHandler h)
		{
			h.HandlePartyCharacterHover(UnitEntityData, value);
		});
	}

	public void ShowBark(string text)
	{
		BarkPartVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkPartVM.HideBark();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		m_IsLevelUpCurrent.Value = manager.TargetUnit == UnitEntityData;
		m_IsLevelUpInProgress.Value = true;
	}

	public void HandleDestroyLevelUpManager()
	{
		m_IsLevelUpCurrent.Value = false;
		m_IsLevelUpInProgress.Value = false;
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		UpdateLevelUpField(UnitEntityData);
	}

	public void HandleUISelectionChanged()
	{
	}

	public void HandleRoleSet(string entityId)
	{
		if (UnitEntityData != null && UnitEntityData.UniqueId.Equals(entityId, StringComparison.Ordinal))
		{
			SetNetAvatar();
		}
	}

	private void SetNetAvatar()
	{
		Sprite value = (UnitEntityData.IsMyNetRole() ? null : UnitEntityData.GetPlayer().GetPlayerIcon());
		m_NetAvatar.Value = value;
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		m_NetAvatar.Value = null;
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		SetNetAvatar();
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
