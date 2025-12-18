using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationGroupConsoleView : JournalNavigationGroupBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonConsole m_ExpandableCollapseMultiButton;

	[SerializeField]
	[UsedImplicitly]
	private JournalNavigationGroupElementConsoleView NavigationGroupElementViewPrefab;

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

	public List<IConsoleEntity> GetSelectableEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		list.Add(m_MultiButton);
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is IConsoleEntity item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
