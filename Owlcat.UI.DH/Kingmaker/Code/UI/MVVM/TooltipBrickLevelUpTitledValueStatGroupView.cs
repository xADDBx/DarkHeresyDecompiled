using Assets.Code.View.UI.MVVM;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpTitledValueStatGroupView : TooltipBaseBrickView<TooltipBrickLevelUpTitledValueStatGroupVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private TooltipElementStatValueView m_StatValuePrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.text = base.ViewModel.Name;
		m_WidgetList.DrawEntries(base.ViewModel.StatGroups, m_StatValuePrefab).AddTo(this);
	}
}
