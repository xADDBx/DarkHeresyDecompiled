using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpStatProgressionView : BrickBaseView<BrickLevelUpStatProgressionVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_UpgradingLimitLabel;

	[SerializeField]
	private List<GameObject> m_PointsSpentChecks;

	[SerializeField]
	private List<TMP_Text> m_BonusTexts;

	[SerializeField]
	private RectTransform m_ChechmarkContainer;

	[Header("Values")]
	[SerializeField]
	private float CheckmarkWidth = 83.25f;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_UpgradingLimitLabel).AddTo(this);
			m_TextHelper.AppendTexts(m_BonusTexts.ToArray());
		}
		base.OnBind();
		for (int i = 0; i < m_PointsSpentChecks.Count; i++)
		{
			m_PointsSpentChecks[i].SetActive(i < base.ViewModel.PointsSpent);
		}
		m_UpgradingLimitLabel.text = UIStrings.Instance.Tooltips.UpgradingLimit.Text;
		for (int j = 0; j < m_BonusTexts.Count; j++)
		{
			int num = (j + 1) * base.ViewModel.StatPerPoint;
			m_BonusTexts[j].text = $"+{num}";
		}
		m_ChechmarkContainer.sizeDelta = new Vector2(CheckmarkWidth * (float)base.ViewModel.PointsTotal, m_ChechmarkContainer.sizeDelta.y);
		m_TextHelper.UpdateTextSize();
	}
}
