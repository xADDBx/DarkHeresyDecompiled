using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugReportDuplicatesConsoleView : BugReportDuplicatesBaseView
{
	[Header("Console Hint")]
	[SerializeField]
	private HintView m_OpenHint;

	[SerializeField]
	private HintView m_MetHint;

	[SerializeField]
	private HintView m_BackHint;

	private ReactiveProperty<bool> m_IsShowHint = new ReactiveProperty<bool>();

	protected void CreateInput()
	{
	}
}
