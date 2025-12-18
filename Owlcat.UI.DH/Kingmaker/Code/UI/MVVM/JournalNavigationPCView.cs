using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationPCView : JournalNavigationBaseView
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatToggle m_ShowCompleteToggle;

	[SerializeField]
	private TextMeshProUGUI m_ShowCompleteLabel;

	[Header("Views")]
	[SerializeField]
	private JournalNavigationGroupPCView m_NavigationGroupViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementPCView m_NavigationOrderViewPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_ShowCompleteToggle.Set(base.ShowCompleted);
		m_ShowCompleteLabel.text = UIStrings.Instance.QuesJournalTexts.ShowCompletedQuests;
		m_ShowCompleteToggle.IsOn.Skip(1).Subscribe(base.OnShowCompletedToggleChanged).AddTo(this);
	}

	public override void DrawEntities()
	{
		base.DrawEntities();
		if (base.ViewModel.ActiveTab.CurrentValue == JournalTab.Quests)
		{
			JournalNavigationGroupVM[] datas = (base.ShowCompleted ? base.ViewModel.NavigationGroups.ToArray() : base.ViewModel.NavigationGroups.Where((JournalNavigationGroupVM q) => q.HasActiveQuests).ToArray());
			base.WidgetList.DrawEntries(datas, m_NavigationGroupViewPrefab);
		}
		ScrollToTop();
	}
}
