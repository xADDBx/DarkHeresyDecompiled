using System.Linq;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationGroupPCView : JournalNavigationGroupBaseView
{
	[Header("Views")]
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonPC m_ExpandableCollapseMultiButton;

	[SerializeField]
	[UsedImplicitly]
	private JournalNavigationGroupElementPCView NavigationGroupElementViewPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		DrawEntities();
		m_ExpandableCollapseMultiButton.SetValue(!base.ViewModel.IsCollapse, isImmediately: true);
		m_ExpandableCollapseMultiButton.IsOn.Subscribe(delegate(bool value)
		{
			base.ViewModel.IsCollapse = !value;
		}).AddTo(this);
	}

	private void DrawEntities()
	{
		JournalQuestVM[] datas = (base.ShowCompletedQuests ? base.ViewModel.Quests.ToArray() : base.ViewModel.Quests.Where((JournalQuestVM q) => q.IsActive).ToArray());
		base.WidgetList.DrawEntries(datas, NavigationGroupElementViewPrefab);
	}
}
