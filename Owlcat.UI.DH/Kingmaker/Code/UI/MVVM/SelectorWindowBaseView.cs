using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SelectorWindowBaseView<TEntityView, TEntityVM> : View<SelectorWindowVM<TEntityVM>> where TEntityView : VirtualListElementViewBase<TEntityVM> where TEntityVM : SelectionGroupEntityVM
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_Header;

	[SerializeField]
	protected VirtualListComponent m_VirtualList;

	[SerializeField]
	protected TEntityView m_SlotPrefab;

	[SerializeField]
	protected InfoSectionView m_InfoSectionView;

	[SerializeField]
	protected CanvasSortingComponent m_SortingComponent;

	protected readonly ReactiveProperty<bool> CanEquip = new ReactiveProperty<bool>(value: true);

	protected readonly ReactiveProperty<bool> m_SelectedEquipped = new ReactiveProperty<bool>();

	protected bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_VirtualList.Initialize(new VirtualListElementTemplate<TEntityVM>(m_SlotPrefab));
			m_InfoSectionView.Initialize();
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		m_FadeAnimator.AppearAnimation();
		m_VirtualList.Subscribe(base.ViewModel.EntitiesCollection).AddTo(this);
		m_InfoSectionView.Bind(base.ViewModel.InfoSectionVM);
		m_SelectedEquipped.Subscribe(delegate(bool value)
		{
			CanEquip.Value = !value && TakeControllableCharacter();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	protected bool TakeControllableCharacter()
	{
		return ((BaseUnitEntity)(base.ViewModel.Slot?.ItemSlot?.Owner))?.CanBeControlled() ?? true;
	}

	protected void OnClose(InputActionEventData inputActionEventData)
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		else
		{
			base.ViewModel.Back();
		}
	}

	protected virtual void OnClose()
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		else
		{
			base.ViewModel.Back();
		}
	}
}
