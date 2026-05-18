using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;

public class AbilitySlotBaseView : View<AbilitySlotVM>
{
	private const string NORMAL = "Normal";

	private const string SELECTED = "Selected";

	[SerializeField]
	protected TextMeshProUGUI m_AbilityName;

	[SerializeField]
	protected Image m_AbilityIcon;

	[SerializeField]
	protected Image m_ModifierIcon;

	[SerializeField]
	protected GameObject m_AvailableModifierAnimation;

	[SerializeField]
	protected Sprite m_EmptyModifierIcon;

	[SerializeField]
	protected Sprite m_LockedModifierIcon;

	[SerializeField]
	protected OwlcatMultiButton m_AbilityButton;

	[SerializeField]
	protected OwlcatMultiButton m_ModifierButton;

	[SerializeField]
	protected OwlcatButton m_ModifierRemoveButton;

	[SerializeField]
	protected GameObject m_ToggleAbilityMark;

	[SerializeField]
	protected GameObject m_HasModifierOnHoverObject;

	private IDisposable m_tooltipDisposable;

	[SerializeField]
	protected DragNDropHandler m_DragNDropHandler;

	protected override void OnBind()
	{
		m_AbilityName.text = base.ViewModel.Name;
		m_AbilityIcon.sprite = base.ViewModel.Icon;
		m_tooltipDisposable = m_ModifierIcon.SetTooltip(base.ViewModel.ModifierTooltip).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_AbilityButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_AbilityButton.OnHoverAsObservable().Subscribe(base.ViewModel.OnHover).AddTo(this);
		m_ModifierButton.OnHoverAsObservable().Subscribe(OnModifierHover).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ModifierRemoveButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetModifier(null);
		}).AddTo(this);
		m_ModifierRemoveButton.SetHint(UIStrings.Instance.ContextMenu.Remove).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(UpdateSlotVisual).AddTo(this);
		base.ViewModel.AppliedModifier.Subscribe(delegate
		{
			UpdateModifierState();
		}).AddTo(this);
		base.ViewModel.CanApplyModifierOnSelect.Subscribe(delegate
		{
			UpdateModifierState();
		}).AddTo(this);
		base.ViewModel.CanApplyModifierOnHover.Subscribe(UpdateSlotVisual).AddTo(this);
		base.ViewModel.HasThisModifier.Subscribe(OnHasModifierOnHover).AddTo(this);
		m_ToggleAbilityMark.SetActive(base.ViewModel.ToggleAbility != null);
		m_AbilityIcon.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_AbilityIcon.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_ModifierIcon.OnBeginDragAsObservable().Subscribe(delegate(PointerEventData d)
		{
			base.ViewModel.OnDrag(d);
		}).AddTo(this);
		m_ModifierIcon.OnDragAsObservable().Subscribe(delegate(PointerEventData d)
		{
			base.ViewModel.OnDrag(d);
		}).AddTo(this);
		m_ModifierIcon.OnEndDragAsObservable().Subscribe(delegate(PointerEventData d)
		{
			base.ViewModel.OnDrag(d, isDragEnd: true);
		}).AddTo(this);
		m_ModifierIcon.OnDropAsObservable().Subscribe(delegate(PointerEventData d)
		{
			base.ViewModel.OnDrag(d);
		}).AddTo(this);
		m_ModifierIcon.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		if (base.ViewModel.IsBrokenOrHeroic)
		{
			m_ModifierIcon.SetHint(UIStrings.Instance.CharacterSheet.NoModificationsHint).AddTo(this);
		}
		SetupDragNDrop();
	}

	private void UpdateModifierState()
	{
		if (base.ViewModel.IsBrokenOrHeroic)
		{
			m_ModifierIcon.sprite = m_LockedModifierIcon;
			return;
		}
		if (base.ViewModel.CanApplyModifierOnSelect.CurrentValue == true)
		{
			m_AvailableModifierAnimation.SetActive(value: true);
			return;
		}
		m_AvailableModifierAnimation.SetActive(value: false);
		m_tooltipDisposable?.Dispose();
		m_tooltipDisposable = m_ModifierIcon.SetTooltip(base.ViewModel.ModifierTooltip).AddTo(this);
		m_ModifierIcon.sprite = ((base.ViewModel.AppliedModifier.CurrentValue == null) ? m_EmptyModifierIcon : base.ViewModel.AppliedModifier.CurrentValue.Tags.FirstOrDefault()?.AbilityIcon);
		if (base.ViewModel.AppliedModifier.CurrentValue == null)
		{
			m_ModifierRemoveButton.gameObject.SetActive(value: false);
		}
	}

	private void UpdateSlotVisual(bool isSelected)
	{
		m_AbilityButton.SetActiveLayer(isSelected ? "Selected" : "Normal");
	}

	private void OnHasModifierOnHover(bool hasModifier)
	{
		m_HasModifierOnHoverObject.SetActive(hasModifier);
	}

	private void SetupDragNDrop()
	{
		if (!(m_DragNDropHandler == null))
		{
			MechanicEntity mechanicEntity = base.ViewModel.Ability?.Owner ?? base.ViewModel.ToggleAbility?.Owner;
			bool flag = base.ViewModel.Ability != null || (base.ViewModel.ToggleAbility != null && (mechanicEntity.IsMyNetRole() || mechanicEntity.InPartyAndControllable()));
			m_DragNDropHandler.CanDrag = flag;
			if (flag)
			{
				m_DragNDropHandler.OnDragEnd.Subscribe(OnDragEnd).AddTo(this);
			}
		}
	}

	private void OnDragEnd(GameObject dropTarget)
	{
		ActionBarSlotAbilityPCView targetSlot = dropTarget.Or(null)?.GetComponent<ActionBarSlotAbilityPCView>();
		if ((bool)targetSlot)
		{
			MechanicEntityFact fact = ((base.ViewModel.Ability != null) ? ((MechanicEntityFact)base.ViewModel.Ability) : ((MechanicEntityFact)base.ViewModel.ToggleAbility));
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.SetSlot(fact, targetSlot.Index);
			});
		}
	}

	public void OnModifierHover(bool value)
	{
		base.ViewModel.OnHover(value);
		m_ModifierRemoveButton.gameObject.SetActive(value && base.ViewModel.AppliedModifier.CurrentValue != null);
	}
}
