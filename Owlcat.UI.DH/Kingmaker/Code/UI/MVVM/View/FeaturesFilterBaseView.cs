using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class FeaturesFilterBaseView : View<FeaturesFilterVM>
{
	[Serializable]
	private struct FilterView
	{
		public OwlcatToggle Toggle;

		public Image Icon;
	}

	[Header("Toggles")]
	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	private FilterView m_None;

	[SerializeField]
	private FilterView m_RecommendedFilter;

	[SerializeField]
	private FilterView m_ChoosedFilter;

	[SerializeField]
	private FilterView m_OffenseFilter;

	[SerializeField]
	private FilterView m_DefenseFilter;

	[SerializeField]
	private FilterView m_SupportFilter;

	[SerializeField]
	private FilterView m_UniversalFilter;

	[SerializeField]
	private FilterView m_ArchetypeFilter;

	[SerializeField]
	private FilterView m_OriginFilter;

	[SerializeField]
	private FilterView m_WarpFilter;

	private Dictionary<FeatureFilterType, FilterView> m_FiltersMap = new Dictionary<FeatureFilterType, FilterView>();

	private Dictionary<FeatureFilterType, LocalizedString> m_FiltersNames = new Dictionary<FeatureFilterType, LocalizedString>();

	private readonly List<FeatureFilterType> m_FiltersOrder = new List<FeatureFilterType>
	{
		FeatureFilterType.None,
		FeatureFilterType.RecommendedFilter,
		FeatureFilterType.FavoritesFilter,
		FeatureFilterType.OffenseFilter,
		FeatureFilterType.DefenseFilter,
		FeatureFilterType.SupportFilter,
		FeatureFilterType.UniversalFilter,
		FeatureFilterType.ArchetypeFilter,
		FeatureFilterType.OriginFilter,
		FeatureFilterType.WarpFilter
	};

	public void Initialize()
	{
		Hide();
		m_FiltersMap = new Dictionary<FeatureFilterType, FilterView>
		{
			{
				FeatureFilterType.None,
				m_None
			},
			{
				FeatureFilterType.RecommendedFilter,
				m_RecommendedFilter
			},
			{
				FeatureFilterType.FavoritesFilter,
				m_ChoosedFilter
			},
			{
				FeatureFilterType.OffenseFilter,
				m_OffenseFilter
			},
			{
				FeatureFilterType.DefenseFilter,
				m_DefenseFilter
			},
			{
				FeatureFilterType.SupportFilter,
				m_SupportFilter
			},
			{
				FeatureFilterType.UniversalFilter,
				m_UniversalFilter
			},
			{
				FeatureFilterType.ArchetypeFilter,
				m_ArchetypeFilter
			},
			{
				FeatureFilterType.OriginFilter,
				m_OriginFilter
			},
			{
				FeatureFilterType.WarpFilter,
				m_WarpFilter
			}
		};
		m_FiltersNames = new Dictionary<FeatureFilterType, LocalizedString>
		{
			{
				FeatureFilterType.None,
				UIStrings.Instance.CharacterSheet.NoneHint
			},
			{
				FeatureFilterType.RecommendedFilter,
				UIStrings.Instance.CharacterSheet.RecommendedFilterHint
			},
			{
				FeatureFilterType.FavoritesFilter,
				UIStrings.Instance.CharacterSheet.FavoritesFilterHint
			},
			{
				FeatureFilterType.OffenseFilter,
				UIStrings.Instance.CharacterSheet.OffenseFilterHint
			},
			{
				FeatureFilterType.DefenseFilter,
				UIStrings.Instance.CharacterSheet.DefenseFilterHint
			},
			{
				FeatureFilterType.SupportFilter,
				UIStrings.Instance.CharacterSheet.SupportFilterHint
			},
			{
				FeatureFilterType.UniversalFilter,
				UIStrings.Instance.CharacterSheet.UniversalFilterHint
			},
			{
				FeatureFilterType.ArchetypeFilter,
				UIStrings.Instance.CharacterSheet.ArchetypeFilterHint
			},
			{
				FeatureFilterType.OriginFilter,
				UIStrings.Instance.CharacterSheet.OriginFilterHint
			},
			{
				FeatureFilterType.WarpFilter,
				UIStrings.Instance.CharacterSheet.WarpFilterHint
			}
		};
		SetupIcons();
	}

	private void SetupIcons()
	{
		foreach (KeyValuePair<FeatureFilterType, FilterView> item in m_FiltersMap)
		{
			item.Deconstruct(out var key, out var value);
			FeatureFilterType filter = key;
			value.Icon.sprite = UIConfig.Instance.FiltersIcons.GetIconFor(filter);
		}
	}

	protected override void OnBind()
	{
		Show();
		SetHints();
		m_FiltersToggleGroup.ActiveToggle.Subscribe(HandleFilterToggle).AddTo(this);
		KeyValuePair<FeatureFilterType, FilterView> keyValuePair = m_FiltersMap.FirstOrDefault((KeyValuePair<FeatureFilterType, FilterView> f) => f.Key == FeaturesFilterVM.ThisSessionFilter);
		if (keyValuePair.Value.Toggle != null)
		{
			keyValuePair.Value.Toggle.Set(value: true);
		}
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void HandleFilterToggle(OwlcatToggle activeToggle)
	{
		if (activeToggle == null)
		{
			return;
		}
		foreach (KeyValuePair<FeatureFilterType, FilterView> item in m_FiltersMap)
		{
			item.Deconstruct(out var key, out var value);
			FeatureFilterType currentFilter = key;
			if (!(value.Toggle != activeToggle))
			{
				base.ViewModel.SetCurrentFilter(currentFilter);
				break;
			}
		}
	}

	private void SetHints()
	{
		foreach (var (key, localizedString2) in m_FiltersNames)
		{
			if (m_FiltersMap.TryGetValue(key, out var value))
			{
				value.Toggle.SetHint(localizedString2).AddTo(this);
			}
		}
	}

	public void SetPrevFilter()
	{
		ShiftActiveToggle(-1);
	}

	public void SetNextFilter()
	{
		ShiftActiveToggle(1);
	}

	private void ShiftActiveToggle(int shiftAmount)
	{
		OwlcatToggle currentToggle = m_FiltersToggleGroup.ActiveToggle.Value;
		FeatureFilterType key = m_FiltersMap.FirstOrDefault((KeyValuePair<FeatureFilterType, FilterView> pair) => pair.Value.Toggle == currentToggle).Key;
		int num = m_FiltersOrder.IndexOf(key);
		if (num >= 0)
		{
			int count = m_FiltersOrder.Count;
			int index = (num + shiftAmount % count + count) % count;
			m_FiltersMap.TryGetValue(m_FiltersOrder.ElementAt(index), out var value);
			value.Toggle.Set(value: true);
		}
	}
}
