using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestObjectiveConsoleView : JournalQuestObjectiveBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonConsole m_ExpandableCollapseMultiButton;

	[Header("Navigation Group Objects")]
	[SerializeField]
	[UsedImplicitly]
	private WidgetList m_WidgetList;

	[SerializeField]
	[UsedImplicitly]
	private JournalQuestObjectiveAddendumConsoleView m_AddendumViewPrefab;

	protected override void OnBind()
	{
		SetupHeader();
		base.OnBind();
		DrawEntities();
	}

	public override void SetupHeader()
	{
		SetHeaderExpandableButtonSettings();
		base.SetupHeader();
	}

	private void SetHeaderExpandableButtonSettings()
	{
		if (!(m_ExpandableCollapseMultiButton == null))
		{
			m_ExpandableCollapseMultiButton.LayerIsOffAlways = base.ViewModel.IsCompleted || base.ViewModel.IsFailed;
			m_ExpandableCollapseMultiButton.LayerIsOnAlways = !base.ViewModel.IsCompleted && !base.ViewModel.IsFailed;
			if (base.ViewModel.IsCompleted || base.ViewModel.IsFailed)
			{
				m_ExpandableCollapseMultiButton.SetActiveLayer(isOn: false);
				m_ExpandableCollapseMultiButton.SetFocus(value: false);
			}
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Addendums.ToArray(), m_AddendumViewPrefab);
	}
}
