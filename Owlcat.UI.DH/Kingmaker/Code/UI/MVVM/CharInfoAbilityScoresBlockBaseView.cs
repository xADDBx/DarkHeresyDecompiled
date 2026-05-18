using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityScoresBlockBaseView : CharInfoComponentWithLevelUpView<CharInfoAbilityScoresBlockVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private View<CharInfoStatVM> m_StatEntryPrefab;

	protected override void RefreshView()
	{
		DrawEntries();
	}

	private void DrawEntries()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.Stats, m_StatEntryPrefab).AddTo(this);
	}
}
