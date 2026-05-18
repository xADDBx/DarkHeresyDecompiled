using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ShipItemsFilterPCView : View<ItemsFilterVM>
{
	protected Dictionary<OwlcatToggle, (ItemsFilterType, float)> m_Filters;

	[SerializeField]
	private GameObject m_Lens;

	[SerializeField]
	private float FilterSwitchAnimationDuration = 0.55f;

	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	protected OwlcatToggle m_None;

	[SerializeField]
	protected OwlcatToggle m_Weapon;

	[SerializeField]
	protected OwlcatToggle m_Other;

	[SerializeField]
	protected OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	private GameObject m_SorterObject;

	[SerializeField]
	private VirtualListComponent m_VirtualList;

	[Header("Search Part")]
	[SerializeField]
	protected ItemsFilterSearchBaseView m_SearchView;

	[SerializeField]
	private float m_ShipNoFilterPosition = -155f;

	[SerializeField]
	private float m_ShipWeaponPosition = -85f;

	[SerializeField]
	private float ShipOtherPosition;

	protected bool m_VisibleSearchBar;

	public virtual void Initialize()
	{
		Hide();
		m_Filters = new Dictionary<OwlcatToggle, (ItemsFilterType, float)>
		{
			{
				m_None,
				(ItemsFilterType.ShipNoFilter, m_ShipNoFilterPosition)
			},
			{
				m_Weapon,
				(ItemsFilterType.ShipWeapon, m_ShipWeaponPosition)
			},
			{
				m_Other,
				(ItemsFilterType.ShipOther, ShipOtherPosition)
			}
		};
		m_SearchView.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		Show();
		m_FiltersToggleGroup.ActiveToggle.Subscribe(HandleFilterToggle).AddTo(this);
		if (m_SorterDropdown != null)
		{
			m_SorterDropdown.Index.Subscribe(delegate
			{
				OnSorterDropdownValueChanged();
			}).AddTo(this);
		}
		SetHints();
		m_SearchView.Or(null)?.Bind(base.ViewModel.ItemsFilterSearchVM);
		if (!m_FiltersToggleGroup.AnyTogglesOn())
		{
			m_None.Set(value: true);
		}
	}

	protected override void OnUnbind()
	{
		SystemSounds.Instance.Selector.Stop.Play();
		SystemSounds.Instance.Selector.LoopStop.Play();
		Hide();
	}

	private void HandleFilterToggle(OwlcatToggle activeToggle)
	{
		if (!(activeToggle == null) && m_Filters.TryGetValue(activeToggle, out var value))
		{
			base.ViewModel.SetCurrentFilter(value.Item1);
			ScrollToTop();
			if (m_Lens.transform.localPosition.x != value.Item2)
			{
				UIUtilityLens.MoveXLensPosition(m_Lens.transform, value.Item2, FilterSwitchAnimationDuration);
			}
		}
	}

	private void SetHints()
	{
		m_None.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAll).AddTo(this);
		m_Weapon.SetHint(UIStrings.Instance.InventoryScreen.FilterTextWeapon).AddTo(this);
		m_Other.SetHint(UIStrings.Instance.InventoryScreen.FilterTextOther).AddTo(this);
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_SorterObject.Or(null)?.SetActive(value: true);
		m_SorterDropdown.Bind(base.ViewModel.SorterDropdownVM);
		OnSorterDropdownValueChanged();
	}

	public void OnSorterDropdownValueChanged()
	{
		int currentValue = m_SorterDropdown.Index.CurrentValue;
		ItemsSorterType currentSorter = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType)))[currentValue];
		base.ViewModel.SetCurrentSorter(currentSorter);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_SorterObject.Or(null)?.SetActive(value: false);
	}

	private void ScrollToTop()
	{
		m_VirtualList.Or(null)?.ScrollController?.ForceScrollToTop();
	}
}
