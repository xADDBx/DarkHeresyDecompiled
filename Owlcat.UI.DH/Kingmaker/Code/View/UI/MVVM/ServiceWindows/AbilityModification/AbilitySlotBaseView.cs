using System.Linq;
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
	protected Sprite m_HoverModifierIcon;

	[SerializeField]
	protected OwlcatMultiButton m_AbilityButton;

	[SerializeField]
	protected GameObject m_ToggleAbilityMark;

	[SerializeField]
	protected DragNDropHandler m_DragNDropHandler;

	protected override void OnBind()
	{
		m_AbilityName.text = base.ViewModel.Name;
		m_AbilityIcon.sprite = base.ViewModel.Icon;
		ObservableSubscribeExtensions.Subscribe(m_AbilityButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_AbilityButton.OnHoverAsObservable().Subscribe(base.ViewModel.OnHover).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(UpdateSlotVisual).AddTo(this);
		base.ViewModel.AppliedModifier.Subscribe(delegate
		{
			UpdateModifierState();
		}).AddTo(this);
		base.ViewModel.CanApplyModifier.Subscribe(delegate
		{
			UpdateModifierState();
		}).AddTo(this);
		base.ViewModel.HasThisModifier.Subscribe(UpdateSlotVisual).AddTo(this);
		m_ToggleAbilityMark.SetActive(base.ViewModel.ToggleAbility != null);
		m_AbilityIcon.SetTooltip(base.ViewModel.Tooltip);
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
		SetupDragNDrop();
	}

	private void UpdateModifierState()
	{
		if (base.ViewModel.CanApplyModifier.CurrentValue == true)
		{
			m_AvailableModifierAnimation.SetActive(value: true);
			return;
		}
		m_AvailableModifierAnimation.SetActive(value: false);
		m_ModifierIcon.SetTooltip(base.ViewModel.ModifierTooltip);
		m_ModifierIcon.sprite = ((base.ViewModel.AppliedModifier.CurrentValue == null) ? m_EmptyModifierIcon : base.ViewModel.AppliedModifier.CurrentValue.Tags.FirstOrDefault().AbilityIcon);
	}

	private void UpdateSlotVisual(bool isSelected)
	{
		m_AbilityButton.SetActiveLayer(isSelected ? "Selected" : "Normal");
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
}
