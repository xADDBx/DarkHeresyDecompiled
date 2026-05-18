using System;
using Code.View.UI.Helpers;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickRankEntrySelectionView : BrickBaseView<BrickRankEntrySelectionVM>
{
	[SerializeField]
	private TMP_Text m_SelectionLabel;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_MainButtonImage;

	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	private IDisposable m_TooltipDisposable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_SelectionLabel).AddTo(this);
		}
		base.OnBind();
		base.ViewModel.RankEntrySelectionVM.SelectedFeature.Subscribe(delegate(RankEntrySelectionFeatureVM featureVM)
		{
			m_CharInfoRankEntryView.Bind(featureVM);
			TooltipBaseTemplate template = ((featureVM != null) ? featureVM.Tooltip.CurrentValue : base.ViewModel.RankEntrySelectionVM.Tooltip);
			m_TooltipDisposable?.Dispose();
			m_TooltipDisposable = m_MainButton.SetTooltip(template).AddTo(this);
			string text = ((featureVM != null) ? featureVM.DisplayName : UIUtilityEncyclopedy.GetGlossaryEntryName(base.ViewModel.RankEntrySelectionVM.GlossaryEntryKey));
			if (featureVM is RankEntrySelectionStatVM rankEntrySelectionStatVM)
			{
				text = rankEntrySelectionStatVM.StatDisplayName;
			}
			m_SelectionLabel.text = text;
		}).AddTo(this);
		base.ViewModel.RankEntrySelectionVM.EntryState.Subscribe(UpdateState).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	private void UpdateState(RankEntryState entryState)
	{
		Sprite entity = UIConfig.Instance.TooltipsConfig.FeatureGroupsIcons.GetEntity(base.ViewModel.RankEntrySelectionVM.FeatureGroup);
		if (entity == null)
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		else
		{
			m_MainButtonImage.sprite = entity;
			m_MainButtonImage.gameObject.SetActive(entryState == RankEntryState.NotSelectable || entryState == RankEntryState.FirstSelectable || entryState == RankEntryState.WaitPreviousToSelect || entryState == RankEntryState.Selectable);
		}
		m_MainButton.SetActiveLayer(entryState.ToString());
	}
}
