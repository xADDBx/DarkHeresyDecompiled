using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Networking;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotPCView : View<ActionBarSlotVM>
{
	[SerializeField]
	private ActionBarSlotType m_ActionBarSlotType;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private bool m_CanBeToggle;

	[ShowIf("m_CanBeToggle")]
	[SerializeField]
	private OwlcatMultiSelectable m_ToggledMark;

	[Header("Hotkey block")]
	[SerializeField]
	private TextMeshProUGUI m_HotkeyText;

	[Header("Convert Block")]
	[SerializeField]
	private bool m_HasConvert;

	[ShowIf("m_HasConvert")]
	[SerializeField]
	private ActionBarConvertedPCView m_ConvertedView;

	[ShowIf("m_HasConvert")]
	[SerializeField]
	private OwlcatMultiButton m_ConvertButton;

	private IDisposable m_Tooltip;

	private string m_BindName;

	private SettingsEntityKeyBindingPair m_Binding;

	private IDisposable m_KeyBind;

	public Observable<Unit> OnLeftClickAsObservable => m_MainButton.OnLeftClickAsObservable();

	public Observable<Unit> OnRightClickAsObservable => m_MainButton.OnRightClickAsObservable();

	public Observable<PointerEventData> OnPointerEnterAsObservable => m_MainButton.OnPointerEnterAsObservable();

	public Observable<PointerEventData> OnPointerExitAsObservable => m_MainButton.OnPointerExitAsObservable();

	public void Awake()
	{
		if (m_CanBeToggle)
		{
			m_ToggledMark.SetActiveLayer("Hidden");
		}
	}

	protected override void OnBind()
	{
		m_Tooltip = m_MainButton.SetTooltip(base.ViewModel.Tooltip);
		m_Tooltip.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			OnMainClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickNotInteractableAsObservable(), delegate
		{
			OnMainClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainButton.OnRightClickAsObservable(), delegate
		{
			OnSupportClick();
		}).AddTo(this);
		m_MainButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnPointerEnter();
		}).AddTo(this);
		m_MainButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnPointerExit();
		}).AddTo(this);
		if (m_CanBeToggle)
		{
			base.ViewModel.IsToggle.CombineLatest(base.ViewModel.IsToggleOn, (bool isToggle, bool isOn) => new { isToggle, isOn }).Subscribe(v =>
			{
				SetToggle(v.isToggle, v.isOn);
			}).AddTo(this);
		}
		if (!m_HasConvert)
		{
			return;
		}
		base.ViewModel.HasConvert.And(base.ViewModel.IsPossibleActive).Subscribe(delegate(bool value)
		{
			m_ConvertedView.gameObject.SetActive(value);
			if (m_ConvertButton != null)
			{
				m_ConvertButton.gameObject.SetActive(value);
			}
		}).AddTo(this);
		base.ViewModel.ConvertedVm.Subscribe(delegate(ActionBarConvertedVM value)
		{
			m_ConvertedView.Bind(value);
			if (m_ConvertButton != null)
			{
				m_ConvertButton.SetActiveLayer((value != null) ? 1 : 0);
			}
		}).AddTo(this);
		if (m_ConvertButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(m_ConvertButton.OnLeftClickAsObservable(), delegate
			{
				ShowConvertRequest();
			}).AddTo(this);
		}
	}

	public void OnMainClick()
	{
		if (base.ViewModel == null || base.ViewModel.IsInCharScreen || base.ViewModel.IsPreciseAttack)
		{
			return;
		}
		if (!Game.Instance.Controllers.SelectionCharacter.IsSingleSelected.Value)
		{
			PhotonManager.Ping.CheckPingCoop(delegate
			{
				if (!string.IsNullOrWhiteSpace(base.ViewModel.MechanicActionBarSlot.KeyName))
				{
					PhotonManager.Ping.PingActionBarAbility(base.ViewModel.MechanicActionBarSlot.KeyName, base.ViewModel.MechanicActionBarSlot.Unit, base.ViewModel.Index);
				}
			});
		}
		else
		{
			base.ViewModel.OnMainClick();
			TooltipHelper.HideTooltip();
		}
	}

	public void OnSupportClick()
	{
		base.ViewModel.OnSupportClick();
	}

	public void OnPointerEnter()
	{
		if (!base.ViewModel.IsInCharScreen)
		{
			base.ViewModel.OnHoverOn();
		}
	}

	public void OnPointerExit()
	{
		if (!base.ViewModel.IsInCharScreen)
		{
			base.ViewModel.OnHoverOff();
		}
	}

	public void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_Tooltip?.Dispose();
		m_Tooltip.AddTo(this);
		m_Tooltip = m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, rectTransform, 0, 0, 0, pivots));
		m_Tooltip.AddTo(this);
	}

	public void SetKeyBinding(int index)
	{
		TryDestroyBinding();
		m_BindName = GetBindName(index);
		m_Binding = GetSettingsEntityKeyBindingPair(index);
		m_KeyBind = Game.Instance.Keyboard.Bind(m_BindName, OnMainClick);
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged += OnBindingChanged;
		}
		SetKeyBindLabel();
	}

	public void ClearKeyBinding()
	{
		TryDestroyBinding();
	}

	private void TryDestroyBinding()
	{
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged -= OnBindingChanged;
		}
		m_KeyBind?.Dispose();
		m_KeyBind = null;
	}

	private void OnBindingChanged(KeyBindingPair obj)
	{
		SetKeyBindLabel();
	}

	private void SetKeyBindLabel()
	{
		if (!(m_HotkeyText == null))
		{
			string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(m_BindName));
			m_HotkeyText.text = stringByBinding;
		}
	}

	private string GetBindName(int index)
	{
		return m_ActionBarSlotType switch
		{
			ActionBarSlotType.Ability => $"ActionBarAbilityButton{index:D2}", 
			ActionBarSlotType.Consumable => $"ActionBarConsumableButton{index:D2}", 
			ActionBarSlotType.WeaponAbility => $"ActionBarWeaponButton{index:D2}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private SettingsEntityKeyBindingPair GetSettingsEntityKeyBindingPair(int index)
	{
		return m_ActionBarSlotType switch
		{
			ActionBarSlotType.Ability => SettingsRoot.Controls.Keybindings.ActionBar.GetAbilityBindingPair($"action-bar-ability-button-{index}"), 
			ActionBarSlotType.Consumable => SettingsRoot.Controls.Keybindings.ActionBar.GetConsumableBindingPair($"action-bar-consumable-button-{index}"), 
			ActionBarSlotType.WeaponAbility => SettingsRoot.Controls.Keybindings.ActionBar.GetWeaponBindingPair($"action-bar-weapon-button-{index}"), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override void OnUnbind()
	{
		TryDestroyBinding();
	}

	private void ShowConvertRequest()
	{
		base.ViewModel.OnShowConvertRequest();
	}

	private void SetToggle(bool isToggle, bool isOn)
	{
		if (!isToggle)
		{
			m_ToggledMark.SetActiveLayer("Hidden");
		}
		else
		{
			m_ToggledMark.SetActiveLayer(isOn ? "ToggleOn" : "ToggleOff");
		}
	}
}
