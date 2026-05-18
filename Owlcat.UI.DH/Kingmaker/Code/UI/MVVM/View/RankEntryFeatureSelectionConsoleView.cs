using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureSelectionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntrySelectionVM>
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
	private HintView m_PrevFilterHint;

	[SerializeField]
	private HintView m_NextFilterHint;

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

	private RankEntrySelectionFeatureVM m_IsFocusedSelection;

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
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_VMCollection.Clear();
		m_FeaturesFilter.Or(null)?.Unbind();
	}

	public void AddInput()
	{
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
				ButtonsSounds.Instance.DoctrineNextButton.Click.Play();
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
}
