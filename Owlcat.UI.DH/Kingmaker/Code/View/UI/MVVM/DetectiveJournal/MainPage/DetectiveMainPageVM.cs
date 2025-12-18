using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;

public class DetectiveMainPageVM : ViewModel
{
	public readonly ObservableList<CaseCardVM> Cases = new ObservableList<CaseCardVM>();

	private readonly ReactiveProperty<bool> m_HasAnyFolder = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowClosedCases = new ReactiveProperty<bool>();

	private readonly Action<BlueprintCase> m_OnCaseCardClick;

	public ReadOnlyReactiveProperty<bool> HasAnyFolder => m_HasAnyFolder;

	public ReadOnlyReactiveProperty<bool> ShowClosedCases => m_ShowClosedCases;

	public DetectiveMainPageVM(Action<BlueprintCase> onCaseCardClick)
	{
		m_OnCaseCardClick = onCaseCardClick;
		m_ShowClosedCases.Value = Game.Instance.Player.UISettings.DetectiveSystemData.ShowClosedCases;
		m_ShowClosedCases.Subscribe(delegate
		{
			UpdateCases();
		}).AddTo(this);
		m_OnCaseCardClick = onCaseCardClick;
	}

	public void ToggleClosedCases()
	{
		m_ShowClosedCases.Value = !m_ShowClosedCases.Value;
		Game.Instance.Player.UISettings.DetectiveSystemData.ShowClosedCases = m_ShowClosedCases.Value;
	}

	private void UpdateCases()
	{
		DetectiveSystem detectiveSystem = Game.Instance.DetectiveSystem;
		Cases.Clear();
		IEnumerable<BlueprintCase> allAvailableCases = detectiveSystem.GetAllAvailableCases();
		bool flag = detectiveSystem.GetUnknownClues().Any();
		IEnumerable<BlueprintCase> source = (m_ShowClosedCases.Value ? allAvailableCases : detectiveSystem.GetCasesWithStatus(CaseStatus.Opened));
		Cases.AddRange(source.Select((BlueprintCase c) => new CaseCardVM(c, m_OnCaseCardClick)));
		if (flag)
		{
			Cases.Add(new CaseCardVM(null, m_OnCaseCardClick));
		}
		m_HasAnyFolder.Value = allAvailableCases.Any() || flag;
	}
}
