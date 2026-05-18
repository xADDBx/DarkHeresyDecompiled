using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterBaseView : View<ItemsFilterVM>
{
	private const float LensThreshold = 0.0001f;

	[Header("Toggles")]
	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	private OwlcatToggle m_None;

	[SerializeField]
	private OwlcatToggle m_Weapon;

	[SerializeField]
	private OwlcatToggle m_Armor;

	[SerializeField]
	private OwlcatToggle m_Accessories;

	[SerializeField]
	private OwlcatToggle m_Usable;

	[SerializeField]
	private OwlcatToggle m_Notable;

	[SerializeField]
	private OwlcatToggle m_Other;

	[SerializeField]
	private OwlcatToggle m_Trash;

	[SerializeField]
	private OwlcatToggle m_BuyBack;

	[Header("Components")]
	[SerializeField]
	private GameObject m_Lens;

	[SerializeField]
	protected OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	private GameObject m_SorterObject;

	[SerializeField]
	private VirtualListComponent m_VirtualList;

	[Header("Search Part")]
	[SerializeField]
	protected ItemsFilterSearchBaseView m_SearchView;

	[Header("Filters")]
	[SerializeField]
	protected List<ItemsFilterType> m_SortedFiltersList = new List<ItemsFilterType>
	{
		ItemsFilterType.NoFilter,
		ItemsFilterType.Weapon,
		ItemsFilterType.Armor,
		ItemsFilterType.Accessories,
		ItemsFilterType.Usable,
		ItemsFilterType.Notable,
		ItemsFilterType.NonUsable,
		ItemsFilterType.BuyBack,
		ItemsFilterType.Trash
	};

	[Header("AvailableItems")]
	[SerializeField]
	protected bool m_ShowToggle;

	[SerializeField]
	private GameObject m_ToggleParent;

	[SerializeField]
	protected OwlcatToggle m_ToggleUnequippable;

	[SerializeField]
	private TextMeshProUGUI m_ToggleLabel;

	[SerializeField]
	private GameObject m_NotificationParent;

	[SerializeField]
	private TextMeshProUGUI m_NotificationLabel;

	[SerializeField]
	private OwlcatMultiButton m_NotificationButton;

	[Header("Values")]
	[SerializeField]
	private float m_LensStartPosition = -185f;

	[SerializeField]
	private float m_LensOffsetDelta = 54.5f;

	[SerializeField]
	private float FilterSwitchAnimationDuration = 0.55f;

	private Dictionary<ItemsFilterType, OwlcatToggle> m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>();

	private ItemsFilterType m_FirstFilter;

	private ItemsFilterType m_LastFilter;

	private AccessibilityTextHelper m_TextHelper;

	private IDisposable m_ShowUneqippableDisposable;

	public virtual void Initialize()
	{
		Hide();
		m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>
		{
			{
				ItemsFilterType.NoFilter,
				m_None
			},
			{
				ItemsFilterType.Weapon,
				m_Weapon
			},
			{
				ItemsFilterType.Armor,
				m_Armor
			},
			{
				ItemsFilterType.Accessories,
				m_Accessories
			},
			{
				ItemsFilterType.Usable,
				m_Usable
			},
			{
				ItemsFilterType.Notable,
				m_Notable
			},
			{
				ItemsFilterType.NonUsable,
				m_Other
			},
			{
				ItemsFilterType.BuyBack,
				m_BuyBack
			},
			{
				ItemsFilterType.Trash,
				m_Trash
			}
		};
		m_SearchView.Or(null)?.Initialize();
		m_FirstFilter = m_SortedFiltersList.FirstOrDefault();
		m_LastFilter = m_SortedFiltersList.LastOrDefault();
		m_TextHelper = new AccessibilityTextHelper(m_ToggleLabel);
	}

	protected override void OnBind()
	{
		Show();
		m_FiltersToggleGroup.ActiveToggle.Subscribe(delegate(OwlcatToggle val)
		{
			HandleFilterToggle(val);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnFilterReset, delegate
		{
			HandleFilterToggle(m_None, fromBuying: true);
		}).AddTo(this);
		m_SorterDropdown.Or(null)?.Index.Subscribe(delegate
		{
			OnSorterDropdownValueChanged();
		}).AddTo(this);
		m_SorterDropdown.Or(null)?.IsOn.Subscribe(OnSorterDropdownStateChanged).AddTo(this);
		SetHints();
		m_SearchView.Or(null)?.Bind(base.ViewModel.ItemsFilterSearchVM);
		m_FiltersMap.GetValueOrDefault(base.ViewModel.CurrentFilter.CurrentValue, m_None).Set(value: true);
		base.ViewModel.CurrentFilter.Subscribe(OnCurrentFilterChanged).AddTo(this);
		base.ViewModel.CurrentSorter.Subscribe(OnCurrentSorterChanged).AddTo(this);
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, OnPrevious).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, OnNext).AddTo(this);
		SetupToggleGroup();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		SystemSounds.Instance.Selector.Stop.Play();
		SystemSounds.Instance.Selector.LoopStop.Play();
		Hide();
		m_TextHelper.Dispose();
		m_ShowUneqippableDisposable?.Dispose();
		m_ShowUneqippableDisposable = null;
	}

	private void SetHints()
	{
		m_None.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAll).AddTo(this);
		m_Weapon.SetHint(UIStrings.Instance.InventoryScreen.FilterTextWeapon).AddTo(this);
		m_Armor.SetHint(UIStrings.Instance.InventoryScreen.FilterTextArmor).AddTo(this);
		m_Accessories.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAcessories).AddTo(this);
		m_Usable.SetHint(UIStrings.Instance.InventoryScreen.FilterTextUsable).AddTo(this);
		m_Notable.SetHint(UIStrings.Instance.InventoryScreen.FilterTextNotable).AddTo(this);
		m_Other.SetHint(UIStrings.Instance.InventoryScreen.FilterTextOther).AddTo(this);
		if ((bool)m_BuyBack)
		{
			m_BuyBack.SetHint(UIStrings.Instance.InventoryScreen.FilterTextBuyBack);
		}
		if ((bool)m_Trash)
		{
			m_Trash.SetHint(UIStrings.Instance.InventoryScreen.FilterTextTrash).AddTo(this);
		}
	}

	private void SetupToggleGroup()
	{
		m_ToggleParent.Or(null)?.SetActive(m_ShowToggle);
		if (!m_ShowToggle)
		{
			return;
		}
		if ((bool)m_ToggleLabel)
		{
			m_ToggleLabel.text = UIStrings.Instance.InventoryScreen.ShowUnavailableItems;
		}
		if ((bool)m_NotificationLabel)
		{
			m_NotificationLabel.text = UIStrings.Instance.InventoryScreen.ShowUnavailableItems;
		}
		if ((bool)m_NotificationParent && (bool)m_NotificationButton)
		{
			base.ViewModel.ShowUnavailable.Subscribe(delegate(bool show)
			{
				m_NotificationParent.SetActive(!show);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_NotificationButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.SetShowUnavailable(show: true);
			}).AddTo(this);
		}
	}

	public void HandleFilterToggle(OwlcatToggle activeToggle, bool fromBuying = false)
	{
		if (activeToggle == null)
		{
			return;
		}
		foreach (var (itemsFilterType2, owlcatToggle2) in m_FiltersMap)
		{
			if (!(owlcatToggle2 == null) && !(owlcatToggle2 != activeToggle))
			{
				base.ViewModel.SetCurrentFilter(itemsFilterType2);
				if (!fromBuying)
				{
					ScrollToTop();
				}
				float num = m_LensStartPosition + (float)m_SortedFiltersList.IndexOf(itemsFilterType2) * m_LensOffsetDelta;
				if (Math.Abs(m_Lens.transform.localPosition.x - num) > 0.0001f)
				{
					UIUtilityLens.MoveXLensPosition(m_Lens.transform, num, FilterSwitchAnimationDuration);
				}
				break;
			}
		}
	}

	public void HandleFilterToggle(ItemsFilterType type)
	{
		base.ViewModel.SetCurrentFilter(type);
		float num = m_LensStartPosition + (float)m_SortedFiltersList.IndexOf(type) * m_LensOffsetDelta;
		if (Math.Abs(m_Lens.transform.localPosition.x - num) > 0.0001f)
		{
			UIUtilityLens.MoveXLensPosition(m_Lens.transform, num, FilterSwitchAnimationDuration);
		}
	}

	public void OnPrevious()
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.CurrentValue == m_FirstFilter)
		{
			base.ViewModel.SetCurrentFilter(m_LastFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.CurrentValue) - 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}

	public void OnNext()
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.CurrentValue == m_LastFilter)
		{
			base.ViewModel.SetCurrentFilter(m_FirstFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.CurrentValue) + 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_SorterObject.Or(null)?.SetActive(value: true);
		m_SorterDropdown.Bind(base.ViewModel.SorterDropdownVM);
		OnSorterDropdownValueChanged();
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_SorterObject.Or(null)?.SetActive(value: false);
	}

	private void OnSorterDropdownValueChanged()
	{
		int currentValue = m_SorterDropdown.Index.CurrentValue;
		ItemsSorterType currentSorter = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType)))[currentValue];
		base.ViewModel.SetCurrentSorter(currentSorter);
	}

	private void OnCurrentFilterChanged(ItemsFilterType filterType)
	{
		OwlcatToggle owlcatToggle = m_FiltersMap[filterType];
		if (!owlcatToggle.IsOn.CurrentValue)
		{
			owlcatToggle.Set(value: true);
		}
	}

	private void OnCurrentSorterChanged(ItemsSorterType sorterType)
	{
		int num = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType))).IndexOf(sorterType);
		if (num != m_SorterDropdown.Index.CurrentValue)
		{
			m_SorterDropdown.SetIndex(num);
		}
	}

	private void OnSorterDropdownStateChanged(bool isUnfolded)
	{
		m_ShowUneqippableDisposable?.Dispose();
		m_ShowUneqippableDisposable = null;
		if (!isUnfolded || !m_ToggleUnequippable || !m_ShowToggle)
		{
			return;
		}
		CompositeDisposable disposables = (CompositeDisposable)(m_ShowUneqippableDisposable = new CompositeDisposable());
		m_ToggleUnequippable.IsOn.Skip(1).Subscribe(delegate(bool value)
		{
			base.ViewModel.SetShowUnavailable(!value);
		}).AddTo(disposables);
		bool toggleIsOn = !base.ViewModel.ShowUnavailable.CurrentValue;
		if (!m_ToggleUnequippable.gameObject.activeInHierarchy)
		{
			ObservableSubscribeExtensions.Subscribe(m_ToggleUnequippable.OnEnableAsObservable().DelayFrame(1).Take(1), delegate
			{
				m_ToggleUnequippable.Set(toggleIsOn);
			}).AddTo(disposables);
		}
		else
		{
			m_ToggleUnequippable.Set(toggleIsOn);
		}
	}

	private void ScrollToTop()
	{
		m_VirtualList.Or(null)?.ScrollController?.ForceScrollToTop();
	}
}
