using System.Collections.Generic;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBuffGroupWidget : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_RootTransform;

	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private float m_HeaderPadding;

	[SerializeField]
	private BuffView m_BuffPrefab;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private GridLayoutGroup m_BuffsGrid;

	private int m_BuffsCount;

	public void SetBuffs(IReadOnlyList<BuffVM> buffs)
	{
		ClearBuffs();
		m_BuffsCount = buffs.Count;
		bool flag = buffs.Count > 0;
		base.gameObject.SetActive(flag);
		if (flag)
		{
			m_WidgetList.DrawEntries(buffs, m_BuffPrefab);
			UpdateHeaderVisibility(buffs.Count);
		}
	}

	public void ClearBuffs()
	{
		m_WidgetList.Clear();
		m_BuffsCount = 0;
	}

	public void SetHeaderText(string header)
	{
		m_HeaderText.SetText(header);
		UpdateHeaderVisibility(m_BuffsCount);
	}

	private void UpdateHeaderVisibility(int buffsCount)
	{
		int num = buffsCount % m_BuffsGrid.constraintCount;
		int num2 = ((num == 0) ? m_BuffsGrid.constraintCount : num);
		float num3 = m_BuffsGrid.cellSize.x * (float)num2;
		float num4 = m_BuffsGrid.spacing.x * (float)num2 - 1f;
		float x = m_RootTransform.sizeDelta.x;
		float num5 = (float)m_BuffsGrid.padding.left + num3 + num4;
		float num6 = m_HeaderText.preferredWidth + m_HeaderPadding - m_HeaderText.rectTransform.anchoredPosition.x;
		bool active = num5 + num6 <= x;
		m_HeaderText.gameObject.SetActive(active);
	}
}
