using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureSelectionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntrySelectionVM>, IUIHighlighter, ISubscriber
{
	[Header("UltimateFeatures")]
	[SerializeField]
	private CharInfoFeatureConsoleView m_UltimateFeatureConsoleView;

	[Header("Filters")]
	[SerializeField]
	private FeaturesFilterBaseView m_FeaturesFilter;

	[SerializeField]
	private TextMeshProUGUI m_NoFeaturesText;

	[Header("Selector")]
	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[Header("Elements")]
	[SerializeField]
	private SeparatorElementView m_SeparatorElementView;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_PrevFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	[SerializeField]
	private RankEntryStatItemCommonView m_RankEntryStatItemCommonView;

	[SerializeField]
	private RankEntryFeatureItemCommonView m_RankEntryFeatureItemCommonView;

	[SerializeField]
	private RankEntryUltimateFeatureUpgradeItemCommonView m_RankEntryUltimateFeatureUpgradeItemCommonView;

	[SerializeField]
	private RankEntryDescriptionView m_RankEntryDescriptionView;

	private Action<bool> m_ReturnAction;

	private readonly ObservableList<VirtualListElementVMBase> m_VMCollection = new ObservableList<VirtualListElementVMBase>();

	private GridConsoleNavigationBehaviour m_Navigation;

	private RankEntrySelectionFeatureVM m_IsFocusedSelection;

	RectTransform IUIHighlighter.RectTransform => RectTransform;

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryFeatureItemCommonView, 0), new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryUltimateFeatureUpgradeItemCommonView, 1), new VirtualListElementTemplate<RankEntrySelectionStatVM>(m_RankEntryStatItemCommonView, 0), new VirtualListElementTemplate<SeparatorElementVM>(m_SeparatorElementView), new VirtualListElementTemplate<RankEntryDescriptionVM>(m_RankEntryDescriptionView));
		m_FeaturesFilter.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_UltimateFeatureConsoleView.SetActiveState(base.ViewModel.UltimateFeature != null);
		m_UltimateFeatureConsoleView.Bind(base.ViewModel.UltimateFeature);
		base.ViewModel.EntryState.Subscribe(delegate
		{
			SetHeader(UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(base.ViewModel.FeatureGroup, base.ViewModel.CanChangeSelection));
		}).AddTo(this);
		m_VirtualList.Subscribe(m_VMCollection).AddTo(this);
		m_FeaturesFilter.Or(null)?.Bind(base.ViewModel.FeaturesFilterVM);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnFilterChange, delegate
		{
			UpdateCollection();
		}).AddTo(this);
		if (base.ViewModel.SelectedFeature.CurrentValue != null)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(base.ViewModel.SelectedFeature.CurrentValue);
		}
		base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate
		{
			UpdateState();
		}).AddTo(this);
		m_NoFeaturesText.text = UIStrings.Instance.CharacterSheet.NoFeaturesInFilter;
		UpdateCollection();
		CreateNavigation();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_VMCollection.Clear();
		m_Navigation?.Clear();
		m_Navigation = null;
		m_FeaturesFilter.Or(null)?.Unbind();
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (base.ViewModel.FeaturesFilterVM == null)
		{
			m_PrevFilterHint?.Dispose();
			m_NextFilterHint?.Dispose();
			return;
		}
		if ((bool)m_PrevFilterHint)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				bool isFocused2 = m_Navigation.IsFocused;
				m_FeaturesFilter.Or(null)?.SetPrevFilter();
				if (isFocused2)
				{
					m_Navigation.FocusOnFirstValidEntity();
				}
			}, 14);
			m_PrevFilterHint.Bind(inputBindStruct).AddTo(this);
			inputBindStruct.AddTo(this);
		}
		if (!m_NextFilterHint)
		{
			return;
		}
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			bool isFocused = m_Navigation.IsFocused;
			m_FeaturesFilter.Or(null)?.SetNextFilter();
			if (isFocused)
			{
				m_Navigation.FocusOnFirstValidEntity();
			}
		}, 15);
		m_NextFilterHint.Bind(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
	}

	private void UpdateCollection()
	{
		m_VMCollection.Clear();
		if (base.ViewModel.FilteredGroupList == null)
		{
			return;
		}
		foreach (VirtualListElementVMBase filteredGroup in base.ViewModel.FilteredGroupList)
		{
			m_VMCollection.Add(filteredGroup);
		}
		m_NoFeaturesText.gameObject.SetActive(!base.ViewModel.FilteredGroupList.Any());
	}

	public override void UpdateState()
	{
		ButtonActive.Value = base.ViewModel.SelectionMadeAndValid && !base.ViewModel.CareerPathVM.ReadOnly.CurrentValue;
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel != null)
		{
			if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
			{
				base.ViewModel.CareerPathVM.SetRankEntry(null);
			}
			else if (base.ViewModel.SelectionMade && base.ViewModel.SelectedFeature.CurrentValue.FocusedState.CurrentValue)
			{
				base.ViewModel.CareerPathVM.SelectNextItem();
				UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
			}
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
		EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
		{
			h.SetFocusOn(null);
		});
	}

	private void CreateNavigation()
	{
		m_Navigation = new GridConsoleNavigationBehaviour();
		if (base.ViewModel.UltimateFeature != null)
		{
			m_Navigation.AddEntityVertical(m_UltimateFeatureConsoleView);
		}
		GridConsoleNavigationBehaviour vListNav = m_VirtualList.GetNavigationBehaviour();
		m_Navigation.AddEntityVertical(vListNav);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			VirtualListElement virtualListElement = m_VirtualList.Elements.FirstOrDefault((VirtualListElement e) => ((e.ConsoleEntityProxy as View<VirtualListElementVMBase>)?.ViewModel as BaseRankEntryFeatureVM)?.FeatureState.CurrentValue == RankFeatureState.Selected);
			if (virtualListElement != null)
			{
				vListNav.SetCurrentEntity(virtualListElement);
			}
			else
			{
				vListNav.SetCurrentEntity(m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement i) => !(i.Data is ExpandableTitleVM)));
			}
			m_Navigation?.SetCurrentEntity(vListNav);
		}).AddTo(this);
		m_Navigation.DeepestFocusAsObservable.Subscribe(delegate(IConsoleEntity value)
		{
			if (value != null)
			{
				EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
				{
					h.SetFocusOn((value as View<VirtualListElementVMBase>)?.ViewModel as BaseRankEntryFeatureVM);
				});
			}
		}).AddTo(this);
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_Navigation == null)
		{
			CreateNavigation();
		}
		return m_Navigation;
	}

	public void StartHighlight(string key)
	{
	}

	public void StopHighlight(string key)
	{
	}

	public void Highlight(string key)
	{
	}

	public void HighlightOnce(string key)
	{
		if (m_VMCollection == null)
		{
			return;
		}
		int itemId = m_VMCollection.FindIndex((VirtualListElementVMBase vm) => (vm as RankEntrySelectionFeatureVM)?.Feature.AssetGuid == key);
		if (itemId < 0)
		{
			return;
		}
		m_VirtualList.ScrollController.ForceScrollToElement(m_VMCollection.ElementAt(itemId));
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			RankEntryFeatureItemCommonView rankEntryFeatureItemCommonView = m_VirtualList.Elements.ElementAt(itemId).View as RankEntryFeatureItemCommonView;
			if (rankEntryFeatureItemCommonView != null)
			{
				rankEntryFeatureItemCommonView.StartHighlight(key);
				m_VirtualList.GetNavigationBehaviour().FocusOnEntityManual(rankEntryFeatureItemCommonView);
			}
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StopHighlight(key);
			});
		}).AddTo(this);
	}
}
