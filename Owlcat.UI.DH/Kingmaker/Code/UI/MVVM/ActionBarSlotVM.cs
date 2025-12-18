using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotVM : ViewModel, IHoverActionBarSlotHandler, ISubscriber, IAbilityTargetSelectionUIHandler, INetPingActionBarAbility, INetLobbyPlayersHandler, INetRoleSetHandler
{
	[Obsolete("WH2-7361")]
	private readonly ReactiveProperty<int> m_AmmoCost = new ReactiveProperty<int>();

	[Obsolete("WH2-7361")]
	private readonly ReactiveProperty<bool> m_IsReload = new ReactiveProperty<bool>();

	[Obsolete("WH2-7361")]
	private readonly ReactiveProperty<int> m_CurrentAmmo = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsPossibleWithoutNetRole;

	private readonly ReactiveProperty<int> m_ResourceCount;

	private readonly ReactiveProperty<bool> m_HasConvert;

	private readonly ReactiveProperty<bool> m_HasAvailableConvert;

	private readonly ReactiveProperty<bool> m_IsPossibleActive;

	private readonly ReactiveProperty<bool> m_CanActivateSlot;

	private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<Sprite> m_ForeIcon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<int> m_ResourceCost = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_ResourceAmount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_ActionPointCost = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsToggle = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsToggleOn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCasting = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Sprite> m_RestrictedByCriticalEffectIcon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_RestrictedByCriticalEffectName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsEmpty = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsOnCooldown = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_CooldownText = new ReactiveProperty<string>();

	private readonly ReactiveCommand<bool> m_OnClickCommand = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<ActionBarConvertedVM> m_ConvertedVm = new ReactiveProperty<ActionBarConvertedVM>(null);

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<bool> m_IsAlerted = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSelectionBusy = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<(NetPlayer player, bool show)> m_CoopPingActionBarSlot = new ReactiveCommand<(NetPlayer, bool)>();

	private readonly ReactiveProperty<bool> m_MoveAbilityMode = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_UpdateDragAndDropState = new ReactiveCommand<Unit>();

	private ItemEntityWeapon m_Weapon;

	private List<AbilityData> m_Conversion;

	private bool m_TempTooltipState;

	private bool m_TargetSelectionStarted;

	private PartAbilityModifiers Ability = new PartAbilityModifiers();

	private Sprite m_OriginIcon;

	private IDisposable m_DelayedUpdate;

	public readonly bool IsInCharScreen;

	public readonly bool IsFake;

	[Obsolete("WH2-7361")]
	public ReadOnlyReactiveProperty<int> AmmoCost => m_AmmoCost;

	[Obsolete("WH2-7361")]
	public ReadOnlyReactiveProperty<bool> IsReload => m_IsReload;

	[Obsolete("WH2-7361")]
	public ReadOnlyReactiveProperty<int> CurrentAmmo => m_CurrentAmmo;

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<Sprite> ForeIcon => m_ForeIcon;

	public ReadOnlyReactiveProperty<int> ResourceCount => m_ResourceCount;

	public ReadOnlyReactiveProperty<int> ActionPointCost => m_ActionPointCost;

	public ReadOnlyReactiveProperty<bool> IsToggle => m_IsToggle;

	public ReadOnlyReactiveProperty<bool> IsToggleOn => m_IsToggleOn;

	public ReadOnlyReactiveProperty<bool> IsCasting => m_IsCasting;

	public ReadOnlyReactiveProperty<bool> IsPossibleActive => m_IsPossibleActive;

	public ReadOnlyReactiveProperty<Sprite> RestrictedByCriticalEffectIcon => m_RestrictedByCriticalEffectIcon;

	public ReadOnlyReactiveProperty<string> RestrictedByCriticalEffectName => m_RestrictedByCriticalEffectName;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> HasConvert => m_HasConvert;

	public ReadOnlyReactiveProperty<bool> IsEmpty => m_IsEmpty;

	public Observable<bool> OnClickCommand => m_OnClickCommand;

	public ReadOnlyReactiveProperty<ActionBarConvertedVM> ConvertedVm => m_ConvertedVm;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public ReadOnlyReactiveProperty<bool> IsAlerted => m_IsAlerted;

	public ReadOnlyReactiveProperty<bool> IsSelectionBusy => m_IsSelectionBusy;

	public Observable<(NetPlayer player, bool show)> CoopPingActionBarSlot => m_CoopPingActionBarSlot;

	public ReadOnlyReactiveProperty<bool> MoveAbilityMode => m_MoveAbilityMode;

	public Observable<Unit> UpdateDragAndDropState => m_UpdateDragAndDropState;

	public ReadOnlyReactiveProperty<bool> CanActivateSlot => m_CanActivateSlot;

	[Obsolete("WH2-7361")]
	public int MaxWeaponAbilityAmmo { get; private set; }

	public bool HasAbilityModification { get; private set; }

	public MechanicActionBarSlot MechanicActionBarSlot { get; private set; }

	public int Index { get; }

	public AbilityData AbilityData
	{
		get
		{
			if (MechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
			{
				return mechanicActionBarSlotAbility.Ability;
			}
			if (MechanicActionBarSlot is MechanicActionBarSlotItem mechanicActionBarSlotItem)
			{
				return mechanicActionBarSlotItem.Ability?.Data;
			}
			return null;
		}
	}

	public bool IsPreciseAttack => Game.Instance.Controllers.PreciseAttackController.HasTarget;

	private ActionBarSlotVM()
	{
		m_CanActivateSlot = new ReactiveProperty<bool>(value: true);
		m_IsPossibleActive = new ReactiveProperty<bool>().AddTo(this);
		m_IsPossibleWithoutNetRole = new ReactiveProperty<bool>().AddTo(this);
		m_ResourceCount = new ReactiveProperty<int>().AddTo(this);
		m_HasConvert = new ReactiveProperty<bool>().AddTo(this);
		m_HasAvailableConvert = new ReactiveProperty<bool>().AddTo(this);
		m_IsPossibleWithoutNetRole.CombineLatest(m_IsPossibleActive, m_ResourceCount, m_HasConvert, m_HasAvailableConvert, CanActivateSlot).Subscribe(delegate(bool canUse)
		{
			m_CanActivateSlot.Value = canUse;
		}).AddTo(this);
		bool CanActivateSlot(bool canActivate, bool canActivateWithoutNetRole, int resourceCount, bool hasConvert, bool convertAvailable)
		{
			if (IsInCharScreen)
			{
				return true;
			}
			if (!canActivate)
			{
				return false;
			}
			if (!(canActivateWithoutNetRole && hasConvert))
			{
				if (canActivateWithoutNetRole)
				{
					return !IsFake;
				}
				return false;
			}
			return resourceCount != 0 && convertAvailable;
		}
	}

	public ActionBarSlotVM(MechanicActionBarSlot abs, int index = -1, bool isInCharScreen = false, ReactiveProperty<bool> moveMode = null)
		: this()
	{
		Index = index;
		IsInCharScreen = isInCharScreen;
		m_MoveAbilityMode = moveMode;
		EventBus.Subscribe(this).AddTo(this);
		SetMechanicSlot(abs);
	}

	public ActionBarSlotVM(WeaponAbility abilityFact, ItemEntityWeapon weapon)
		: this()
	{
		IsFake = true;
		ReactiveProperty<Sprite> icon = m_Icon;
		BlueprintAbility ability = abilityFact.Ability;
		icon.Value = ((ability != null) ? ObjectExtensions.Or(ability.Icon, UIConfig.Instance.UIIcons.DefaultAbilityIcon) : null);
		m_ResourceCount.Value = -1;
		m_ActionPointCost.Value = abilityFact.AP;
		m_Tooltip.Value = new TooltipTemplateDataProvider(abilityFact.Ability);
		m_IsReload.Value = UIUtilityItem.IsReload(abilityFact.Ability);
		m_Weapon = weapon;
		MaxWeaponAbilityAmmo = 0;
		m_CurrentAmmo.Value = 0;
	}

	protected override void OnDispose()
	{
		m_DelayedUpdate?.Dispose();
		m_DelayedUpdate = null;
		MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
		if (mechanicActionBarSlot != null && mechanicActionBarSlot.HoverState)
		{
			MechanicActionBarSlot.OnHover(state: false);
		}
		CloseConvert();
	}

	public void SetMechanicSlot(MechanicActionBarSlot abs)
	{
		if (abs != MechanicActionBarSlot)
		{
			MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
			if (mechanicActionBarSlot != null && mechanicActionBarSlot.HoverState)
			{
				MechanicActionBarSlot.OnHover(state: false);
			}
			m_IsEmpty.Value = abs is MechanicActionBarSlotEmpty;
			MechanicActionBarSlot = abs;
			m_Conversion = (from a in MechanicActionBarSlot.GetConvertedAbilityData()
				where a.IsVisible()
				select a).ToList();
			m_Icon.Value = MechanicActionBarSlot.GetIcon() ?? UIConfig.Instance.UIIcons.DefaultAbilityIcon;
			m_ForeIcon.Value = MechanicActionBarSlot.GetForeIcon();
			m_HasConvert.Value = m_Conversion.Count > 0;
			m_Tooltip.Value = MechanicActionBarSlot.GetTooltipTemplate();
			MechanicActionBarSlotAbility mechanicActionBarSlotAbility = abs as MechanicActionBarSlotAbility;
			m_IsReload.Value = UIUtilityItem.IsReload(mechanicActionBarSlotAbility?.Ability);
			m_Weapon = mechanicActionBarSlotAbility?.Ability?.SourceItem as ItemEntityWeapon;
			MaxWeaponAbilityAmmo = (IsReload.CurrentValue ? UIUtilityItem.GetMaxAbilityAmmo(m_Weapon) : 0);
			UpdateResources();
			m_DelayedUpdate?.Dispose();
			m_DelayedUpdate = DelayedInvoker.InvokeInTime(delegate
			{
				m_DelayedUpdate.Dispose();
				m_DelayedUpdate = null;
				UpdateResources();
			}, 0.5f);
		}
	}

	public void UpdateResources()
	{
		try
		{
			if (MechanicActionBarSlot?.Unit == null)
			{
				return;
			}
			MechanicActionBarSlot.UpdateResourceCount();
			MechanicActionBarSlot.UpdateResourceCost();
			MechanicActionBarSlot.UpdateResourceAmount();
			m_ResourceCount.Value = MechanicActionBarSlot.GetResource();
			m_ResourceCost.Value = MechanicActionBarSlot.GetResourceCost();
			m_ResourceAmount.Value = MechanicActionBarSlot.GetResourceAmount();
			m_AmmoCost.Value = 0;
			m_IsCasting.Value = MechanicActionBarSlot.IsCasting();
			m_IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
			m_IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
			m_RestrictedByCriticalEffectIcon.Value = null;
			m_ActionPointCost.Value = MechanicActionBarSlot.ActionPointCost();
			m_HasAvailableConvert.Value = m_Conversion.Any((AbilityData abilityData) => abilityData.IsAvailable);
			MechanicActionBarSlotToggleAbility mechanicActionBarSlotToggleAbility = MechanicActionBarSlot as MechanicActionBarSlotToggleAbility;
			m_IsToggle.Value = mechanicActionBarSlotToggleAbility != null;
			m_IsToggleOn.Value = mechanicActionBarSlotToggleAbility?.Ability.Enabled ?? false;
			if (AbilityData == null)
			{
				return;
			}
			m_IsSelected.Value = Game.Instance.Controllers.SelectedAbilityHandler != null && Game.Instance.Controllers.SelectedAbilityHandler.Ability == AbilityData;
			m_IsOnCooldown.Value = AbilityData.IsOnCooldown;
			m_CooldownText.Value = ((AbilityData.Cooldown > 0) ? AbilityData.Cooldown.ToString() : string.Empty);
			if (!MechanicActionBarSlot.IsPossibleActiveWithoutNetRole && AbilityData.AbilityCasterRestriction is AbilityCasterStatGreaterOrEqual10 abilityCasterStatGreaterOrEqual)
			{
				foreach (Buff buff in MechanicActionBarSlot.Unit.Buffs)
				{
					if (buff.Blueprint.CriticalEffect && buff.GetComponent<AddStatModifier>()?.Stat == abilityCasterStatGreaterOrEqual.Stat)
					{
						m_RestrictedByCriticalEffectIcon.Value = buff.Blueprint.Icon;
						m_RestrictedByCriticalEffectName.Value = buff.Blueprint.LocalizedName;
					}
				}
			}
			if (Ability == null)
			{
				Ability = MechanicActionBarSlot.Unit.GetOrCreate<PartAbilityModifiers>();
			}
			HasAbilityModification = Ability.AddedModifiers.Any((PartAbilityModifiers.AddedEntry value) => value.Ability == AbilityData.OriginalBlueprint);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't update resource for MechanicActionBarSlot: \n {arg}");
		}
		finally
		{
			m_CanActivateSlot.ForceNotify();
		}
	}

	public void OnMainClick()
	{
		if (MechanicActionBarSlot == null || PhotonManager.Ping.CheckPingCoop(delegate
		{
			if (!string.IsNullOrWhiteSpace(MechanicActionBarSlot.KeyName))
			{
				PhotonManager.Ping.PingActionBarAbility(MechanicActionBarSlot.KeyName, MechanicActionBarSlot.Unit, Index);
			}
		}))
		{
			return;
		}
		MechanicActionBarSlot.PlaySound();
		if (MechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.Ability.Blueprint.HasVariants && mechanicActionBarSlotAbility.IsPossibleActive)
		{
			OnShowConvertRequest();
			return;
		}
		if (MechanicActionBarSlot.IsPossibleActive)
		{
			m_OnClickCommand.Execute(parameter: false);
		}
		MechanicActionBarSlot.OnClick();
		MechanicActionBarSlotToggleAbility mechanicActionBarSlotToggleAbility = MechanicActionBarSlot as MechanicActionBarSlotToggleAbility;
		m_IsToggle.Value = mechanicActionBarSlotToggleAbility != null;
		m_IsToggleOn.Value = mechanicActionBarSlotToggleAbility?.Ability.Enabled ?? false;
	}

	public void OnSupportClick()
	{
		m_OnClickCommand.Execute(parameter: true);
	}

	public void OnHoverOn()
	{
		MechanicActionBarSlot?.OnHover(state: true);
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot);
		});
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerEnterActionBarSlot(MechanicActionBarSlot);
		});
	}

	public void OnHoverOff()
	{
		MechanicActionBarSlot?.OnHover(state: false);
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot);
		});
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerExitActionBarSlot(MechanicActionBarSlot);
		});
	}

	public void OnShowConvertRequest()
	{
		HandleConvertRequest(CreateConvertSlot);
		MechanicActionBarSlotSpontaneusConvertedSpell CreateConvertSlot(AbilityData data)
		{
			return new MechanicActionBarSlotSpontaneusConvertedSpell
			{
				Spell = data,
				Unit = MechanicActionBarSlot.Unit
			};
		}
	}

	private void HandleConvertRequest(Func<AbilityData, MechanicActionBarSlotSpontaneusConvertedSpell> createSlot)
	{
		if (ConvertedVm.CurrentValue == null)
		{
			if (m_Conversion.Count != 0)
			{
				m_ConvertedVm.Value = new ActionBarConvertedVM(m_Conversion.Select(createSlot).ToList(), CloseConvert).AddTo(this);
				m_Tooltip.Value = null;
			}
		}
		else
		{
			CloseConvert();
			m_Tooltip.Value = MechanicActionBarSlot.GetTooltipTemplate();
		}
	}

	private void CloseConvert()
	{
		ConvertedVm.CurrentValue?.Dispose();
		m_ConvertedVm.Value = null;
	}

	public void CloseConvertsOnTurnStart()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			CloseConvert();
		}
	}

	public void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability)
	{
	}

	public void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability)
	{
	}

	public void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability != MechanicActionBarSlot)
		{
			TryTurnAlertOn((ability as MechanicActionBarSlotAbility)?.Ability);
		}
	}

	public void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			m_IsAlerted.Value = false;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		if (ability == AbilityData)
		{
			m_IsSelected.Value = true;
		}
		if (CheckAbilityHasAttackAbilityGroupCooldown(ability.Blueprint) && (!(MechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility) || !(mechanicActionBarSlotAbility.Ability == ability)))
		{
			m_TargetSelectionStarted = true;
			TryTurnAlertOn(ability);
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_IsSelected.Value = false;
		m_TargetSelectionStarted = false;
		m_IsAlerted.Value = false;
	}

	private void TryTurnAlertOn(AbilityData slotAbility)
	{
		if (!(slotAbility == null) && !(AbilityData == null) && Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			m_IsAlerted.Value = slotAbility.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup g) => AbilityData.Blueprint.AbilityGroups.Contains(g) && g.CooldownInRounds > 0);
		}
	}

	private bool CheckAbilityHasAttackAbilityGroupCooldown(BlueprintAbilityWrapper blueprintAbility)
	{
		foreach (BlueprintAbilityGroup abilityGroup in blueprintAbility.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				return true;
			}
		}
		return false;
	}

	public void ChooseAbility()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.ChooseAbilityToSlot(Index);
		});
	}

	public void ClearSlot()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.DeleteSlot(Index);
		});
	}

	public void MoveAbility()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.SetMoveAbilityMode(on: true);
		});
	}

	public void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex)
	{
		if (!string.IsNullOrWhiteSpace(keyName) && characterEntityRef == MechanicActionBarSlot.Unit && slotIndex == Index)
		{
			m_CoopPingActionBarSlot.Execute((player, keyName == MechanicActionBarSlot.KeyName));
		}
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		m_IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		m_IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		m_IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		m_IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
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

	public void HandleRoleSet(string entityId)
	{
		m_IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		m_IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
		m_UpdateDragAndDropState.Execute();
	}

	public void SetSelectionBusy(bool isBusy)
	{
		m_IsSelectionBusy.Value = isBusy;
	}

	public void OverrideIcon(Sprite icon)
	{
		if (icon == null)
		{
			m_Icon.Value = m_OriginIcon;
			m_OriginIcon = null;
			return;
		}
		if ((object)m_OriginIcon == null)
		{
			m_OriginIcon = Icon.CurrentValue;
		}
		m_Icon.Value = icon;
	}
}
