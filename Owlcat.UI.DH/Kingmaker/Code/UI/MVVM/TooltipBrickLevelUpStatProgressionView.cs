using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpStatProgressionView : TooltipBaseBrickView<TooltipBrickLevelUpStatProgressionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_UpgradingLimitLabel;

	[SerializeField]
	private List<GameObject> m_PointsSpentChecks;

	protected override void OnBind()
	{
		base.OnBind();
		for (int i = 0; i < m_PointsSpentChecks.Count; i++)
		{
			m_PointsSpentChecks[i].SetActive(i < base.ViewModel.PointsSpent);
		}
		m_UpgradingLimitLabel.text = UIStrings.Instance.Tooltips.UpgradingLimit.Text;
	}
}
