using System.Collections.Generic;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhasePantographItemView : SelectionGroupEntityView<CharGenAttributesItemVM>
{
	[SerializeField]
	private ScrambledTMP m_DisplayName;

	[SerializeField]
	private CharGenAttributesPhasePantographItemRankWidget m_RankWidget;

	[SerializeField]
	private RectTransform m_RanksContainer;

	[SerializeField]
	private ScrambledTMP m_StatValue;

	[SerializeField]
	private ScrambledTMP m_ShortLabel;

	[SerializeField]
	private Image m_StatImage;

	[SerializeField]
	private GameObject m_RecommendedMark;

	private readonly List<CharGenAttributesPhasePantographItemRankWidget> m_RankWidgets = new List<CharGenAttributesPhasePantographItemRankWidget>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateRankWidgets();
		m_DisplayName.SetText(string.Empty, base.ViewModel.DisplayName);
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate(int value)
		{
			m_StatValue.SetText(m_StatValue.Text, value.ToString());
		}));
		AddDisposable(base.ViewModel.StatRanks.Subscribe(delegate(int value)
		{
			for (int i = 0; i < m_RankWidgets.Count; i++)
			{
				m_RankWidgets[i].SetState(i < value);
			}
		}));
		if ((bool)m_RecommendedMark)
		{
			AddDisposable(base.ViewModel.IsRecommended.Subscribe(delegate(bool value)
			{
				m_RecommendedMark.SetActive(value);
			}));
		}
		m_ShortLabel.SetText(m_ShortLabel.Text, UIUtilityText.GetStatShortName(base.ViewModel.StatType));
	}

	private void CreateRankWidgets()
	{
		if (m_RankWidgets.Count <= 0)
		{
			for (int i = 0; i < 2; i++)
			{
				CharGenAttributesPhasePantographItemRankWidget item = Object.Instantiate(m_RankWidget, m_RanksContainer, worldPositionStays: false);
				m_RankWidgets.Add(item);
			}
		}
	}
}
