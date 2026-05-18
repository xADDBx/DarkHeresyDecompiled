using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpTitledValueStatGroupView : BrickBaseView<BrickLevelUpTitledValueStatGroupVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private TooltipElementStatValueView m_StatValuePrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title).AddTo(this);
		}
		base.OnBind();
		m_Title.text = base.ViewModel.Name;
		m_WidgetList.DrawEntries(base.ViewModel.StatGroups, m_StatValuePrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
