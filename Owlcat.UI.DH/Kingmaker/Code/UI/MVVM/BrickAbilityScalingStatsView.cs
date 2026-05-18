using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScalingStatsView : BrickBaseView<BrickAbilityScalingStatsVM>
{
	[SerializeField]
	private RectTransform m_StatsContainer;

	[SerializeField]
	private AbilityStatWidget m_StatWidgetPrefab;

	[SerializeField]
	private int m_MaxStatsCount = 4;

	[SerializeField]
	private int m_MaxStatsToShowDescriptionCount = 2;

	[SerializeField]
	private GameObject[] m_StatPlaceholders;

	[SerializeField]
	private GameObject m_DescriptionBlock;

	[SerializeField]
	private TMP_Text m_DescriptionText;

	private readonly List<AbilityStatWidget> m_StatWidgets = new List<AbilityStatWidget>();

	protected override void OnBind()
	{
		base.OnBind();
		int num = Mathf.Min(m_MaxStatsCount, base.ViewModel.Stats.Count);
		for (int i = 0; i < num; i++)
		{
			AbilityStatData statData = base.ViewModel.Stats[i];
			AbilityStatWidget widget = WidgetFactory.GetWidget(m_StatWidgetPrefab);
			widget.transform.SetParent(m_StatsContainer, worldPositionStays: false);
			widget.Setup(statData.StatName, statData.StatValue, i == 0 && num < m_MaxStatsCount);
			widget.SetTooltip(base.ViewModel.GetStatTooltip(statData)).AddTo(this);
			m_StatWidgets.Add(widget);
		}
		bool flag = num <= m_MaxStatsToShowDescriptionCount;
		m_DescriptionBlock.SetActive(flag);
		if (flag)
		{
			m_DescriptionText.SetText(base.ViewModel.DescriptionText);
		}
		SetupPlaceholders(num, flag);
	}

	protected override void OnUnbind()
	{
		m_StatWidgets.ForEach(WidgetFactory.DisposeWidget);
		m_StatWidgets.Clear();
	}

	private void SetupPlaceholders(int statsCount, bool showDescription)
	{
		int b = (showDescription ? (m_MaxStatsCount - statsCount - 1) : (m_MaxStatsCount - statsCount));
		int num = Mathf.Min(m_StatPlaceholders.Length, b);
		for (int i = 0; i < num; i++)
		{
			m_StatPlaceholders[i].SetActive(value: true);
		}
		for (int j = num; j < m_StatPlaceholders.Length; j++)
		{
			m_StatPlaceholders[j].SetActive(value: false);
		}
	}
}
