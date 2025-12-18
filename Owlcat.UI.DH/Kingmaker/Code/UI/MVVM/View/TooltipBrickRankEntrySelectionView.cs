using System.Linq;
using Code.View.UI.Helpers;
using Code.View.UI.UIUtils;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickRankEntrySelectionView : TooltipBaseBrickView<TooltipBrickRankEntrySelectionVM>
{
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_MainButtonImage;

	[SerializeField]
	private RankEntrySelectionStateSprites[] m_StateSprites;

	[SerializeField]
	private TextMeshProUGUI m_SelectionLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_SelectionLabel);
		base.OnBind();
		base.ViewModel.RankEntrySelectionVM.SelectedFeature.Subscribe(delegate(RankEntrySelectionFeatureVM featureVM)
		{
			m_CharInfoRankEntryView.Bind(featureVM);
			if (featureVM != null)
			{
				m_MainButton.SetTooltip(featureVM.Tooltip).AddTo(this);
			}
			else
			{
				m_MainButton.SetTooltip(base.ViewModel.RankEntrySelectionVM.Tooltip);
			}
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

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}

	private void UpdateState(RankEntryState entryState)
	{
		RankEntrySelectionStateSprites rankEntrySelectionStateSprites = m_StateSprites.FirstOrDefault((RankEntrySelectionStateSprites p) => p.FeatureGroup == base.ViewModel.RankEntrySelectionVM.FeatureGroup);
		if (rankEntrySelectionStateSprites == null)
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		else if (entryState == RankEntryState.NotSelectable || entryState == RankEntryState.FirstSelectable || entryState == RankEntryState.WaitPreviousToSelect || entryState == RankEntryState.Selectable)
		{
			m_MainButtonImage.sprite = rankEntrySelectionStateSprites.Icon;
			m_MainButtonImage.gameObject.SetActive(value: true);
		}
		else
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		m_MainButton.SetActiveLayer(entryState.ToString());
	}
}
