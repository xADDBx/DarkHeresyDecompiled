using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugReportDuplicatesConsoleView : BugReportDuplicatesBaseView
{
	[Header("Console Hint")]
	[SerializeField]
	private ConsoleHint m_OpenHint;

	[SerializeField]
	private ConsoleHint m_MetHint;

	[SerializeField]
	private ConsoleHint m_BackHint;

	private ReactiveProperty<bool> m_IsShowHint = new ReactiveProperty<bool>();

	protected override void CreateInput()
	{
		base.CreateInput();
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		m_BackHint.Bind(InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9)).AddTo(this);
		m_BackHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_OpenHint.Bind(InputLayer.AddButton(delegate
		{
			OpenUrl();
		}, 8, m_IsShowHint)).AddTo(this);
		m_OpenHint.SetLabel(UIStrings.Instance.UIBugReport.OpenJiraTask);
		m_MetHint.Bind(InputLayer.AddButton(delegate
		{
			OpenMet();
		}, 10, m_IsShowHint)).AddTo(this);
		m_MetHint.SetLabel(UIStrings.Instance.UIBugReport.OpenMet);
	}

	protected override void UpdateNavigation()
	{
		base.UpdateNavigation();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_IsShowHint.Value = entity is BugDuplicateItemView;
	}

	private void OpenUrl()
	{
		(m_NavigationBehaviour.CurrentEntity as BugDuplicateItemView)?.OpenUrl();
	}

	private void OpenMet()
	{
		(m_NavigationBehaviour.CurrentEntity as BugDuplicateItemView)?.OpenMet();
	}
}
