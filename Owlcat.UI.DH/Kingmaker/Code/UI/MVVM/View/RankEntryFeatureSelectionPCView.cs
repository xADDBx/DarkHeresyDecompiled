using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureSelectionPCView : BaseCareerPathSelectionTabPCView<RankEntrySelectionVM>, IUIHighlighter, ISubscriber
{
	[Header("UltimateFeatures")]
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_UltimateFeaturePCView;

	[Header("Filters")]
	[SerializeField]
	private FeaturesFilterBaseView m_FeaturesFilter;

	[SerializeField]
	private OwlcatMultiButton m_ShowUnavailableButton;

	[SerializeField]
	private TextMeshProUGUI m_NoFeaturesText;

	[Header("Selector")]
	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[Header("Elements")]
	[SerializeField]
	private SeparatorElementView m_SeparatorElementView;

	[SerializeField]
	private RankEntryStatItemCommonView m_RankEntryStatItemCommonView;

	[SerializeField]
	private RankEntryFeatureItemCommonView m_RankEntryFeatureItemCommonView;

	[SerializeField]
	private RankEntryUltimateFeatureUpgradeItemCommonView m_RankEntryUltimateFeatureUpgradeItemCommonView;

	[SerializeField]
	private RankEntryDescriptionView m_RankEntryDescriptionView;

	private Action<bool> m_ReturnAction;

	private IDisposable m_HintsDisposables;

	private readonly ObservableList<VirtualListElementVMBase> m_VMCollection = new ObservableList<VirtualListElementVMBase>();

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
		m_UltimateFeaturePCView.SetActiveState(base.ViewModel.UltimateFeature != null);
		m_UltimateFeaturePCView.Bind(base.ViewModel.UltimateFeature);
		base.ViewModel.EntryState.Subscribe(delegate
		{
			SetHeader(UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(base.ViewModel.FeatureGroup, base.ViewModel.CanChangeSelection));
		}).AddTo(this);
		m_VirtualList.Subscribe(m_VMCollection).AddTo(this);
		m_FeaturesFilter.Or(null)?.Bind(base.ViewModel.FeaturesFilterVM);
		if ((bool)m_ShowUnavailableButton)
		{
			m_ShowUnavailableButton.gameObject.SetActive(base.ViewModel.FeaturesFilterVM != null);
			UpdateUnavailableFeaturesButtonHint();
			ObservableSubscribeExtensions.Subscribe(m_ShowUnavailableButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.ToggleShowUnavailableFeatures();
				UpdateUnavailableFeaturesButtonHint();
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnFilterChange, delegate
		{
			UpdateCollection();
		}).AddTo(this);
		if (base.ViewModel.SelectedFeature.CurrentValue != null)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(base.ViewModel.SelectedFeature.CurrentValue);
		}
		SetNextButtonLabel(UIStrings.Instance.CharGen.Next);
		SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
		SetFinishButtonLabel(UIStrings.Instance.Tutorial.Complete);
		SetButtonSound(ButtonSoundsEnum.DoctrineNextSound);
		base.ViewModel.CareerPathVM.CanCommit.Subscribe(base.SetFinishInteractable).AddTo(this);
		base.ViewModel.CareerPathVM.CanCommit.CombineLatest(base.ViewModel.CareerPathVM.PointerItem, (bool canCommit, IRankEntrySelectItem pointerItem) => canCommit && pointerItem == null).Subscribe(delegate(bool value)
		{
			base.CanCommit = value;
			SetFinishInteractable(value);
		}).AddTo(this);
		base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			SetButtonVisibility(!ro);
		}).AddTo(this);
		SetupNoFeaturesText();
		UpdateCollection();
		EventBus.Subscribe(this).AddTo(this);
		TextHelper.AppendTexts(m_NoFeaturesText);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue)
		{
			SetButtonVisibility(value: false);
		}
		else
		{
			UpdateState();
		}
		m_VMCollection.Clear();
		m_FeaturesFilter.Or(null)?.Unbind();
		m_HintsDisposables?.Dispose();
		m_HighlightButton.Or(null)?.gameObject.SetActive(value: false);
	}

	private void UpdateCollection()
	{
		m_VMCollection.Clear();
		foreach (VirtualListElementVMBase filteredGroup in base.ViewModel.FilteredGroupList)
		{
			m_VMCollection.Add(filteredGroup);
		}
		m_NoFeaturesText.gameObject.SetActive(!Enumerable.Any(base.ViewModel.FilteredGroupList));
	}

	public override void UpdateState()
	{
		bool flag = base.ViewModel.CanSelect() && base.ViewModel.SelectionMadeAndValid && base.ViewModel.CareerPathVM.LastEntryToUpgrade != base.ViewModel;
		SetNextButtonInteractable(flag);
		m_HighlightButton.Or(null)?.gameObject.SetActive(!flag);
		bool backButtonInteractable = base.ViewModel.CareerPathVM.FirstEntryToUpgrade != base.ViewModel;
		SetBackButtonInteractable(backButtonInteractable);
		HintText.Value = GetHintText();
	}

	private string GetHintText()
	{
		if (base.ViewModel.SelectionMadeAndValid)
		{
			return string.Empty;
		}
		return UIStrings.Instance.CharacterSheet.SelectFeatureButtonHint.Text;
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
		{
			base.ViewModel.CareerPathVM.SetRankEntry(null);
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.CareerPathVM.Commit();
	}

	private void SetupNoFeaturesText()
	{
		if (base.ViewModel.HasFeatures)
		{
			m_NoFeaturesText.text = UIStrings.Instance.CharacterSheet.NoFeaturesInFilter;
			return;
		}
		string text = base.ViewModel.FeatureGroup switch
		{
			FeatureGroup.FirstCareerTalent => UIStrings.Instance.CharacterSheet.AscensionFirstCareerTalentFeatureGroupDescription, 
			FeatureGroup.FirstCareerAbility => UIStrings.Instance.CharacterSheet.AscensionFirstCareerAbilityFeatureGroupDescription, 
			FeatureGroup.SecondCareerTalent => UIStrings.Instance.CharacterSheet.AscensionSecondCareerTalentFeatureGroupDescription, 
			FeatureGroup.SecondCareerAbility => UIStrings.Instance.CharacterSheet.AscensionSecondCareerAbilityFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerTalent => UIStrings.Instance.CharacterSheet.AscensionFirstOrSecondCareerTalentFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerAbility => UIStrings.Instance.CharacterSheet.AscensionFirstOrSecondCareerAbilityFeatureGroupDescription, 
			_ => UIStrings.Instance.CharacterSheet.NoFeaturesInFilter, 
		};
		m_NoFeaturesText.text = text;
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
			if (base.ViewModel.ContainsFeature(key) && !Game.Instance.Player.UISettings.ShowUnavailableFeatures)
			{
				base.ViewModel.ToggleShowUnavailableFeatures();
				HighlightOnce(key);
			}
			return;
		}
		m_VirtualList.ScrollController.ForceScrollToElement(m_VMCollection.ElementAt(itemId));
		DelayedInvoker.InvokeInFrames(delegate
		{
			(m_VirtualList.Elements.ElementAt(itemId).View as RankEntryFeatureItemCommonView)?.StartHighlight(key);
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StopHighlight(key);
			});
		}, 1);
	}

	private void UpdateUnavailableFeaturesButtonHint()
	{
		m_HintsDisposables?.Dispose();
		string text = (Game.Instance.Player.UISettings.ShowUnavailableFeatures ? UIStrings.Instance.CharacterSheet.HideUnavailableFeaturesHint : UIStrings.Instance.CharacterSheet.ShowUnavailableFeaturesHint);
		m_HintsDisposables = m_ShowUnavailableButton.SetHint(text);
	}
}
