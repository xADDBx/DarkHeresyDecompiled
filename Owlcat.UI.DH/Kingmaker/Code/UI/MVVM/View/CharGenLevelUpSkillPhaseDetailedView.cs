using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpSkillPhaseDetailedView : CharGenLevelUpPhaseStatsDetailedView<CharGenLevelUpSkillPhaseVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private LevelUpSkillLinkedAttributeView m_ItemViewPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.BaseAttributeList, m_ItemViewPrefab);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_WidgetList.Clear();
	}
}
