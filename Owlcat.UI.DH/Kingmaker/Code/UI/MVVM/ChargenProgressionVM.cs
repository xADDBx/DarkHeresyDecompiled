using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionVM : ViewModel
{
	public static readonly int MAX_LEVELS = 25;

	private List<ChargenProgressionFeatureVM> m_FeatureRowVMs = new List<ChargenProgressionFeatureVM>();

	private ReadOnlyReactiveProperty<LevelUpManager> m_LevelUpManager;

	private ReactiveProperty<ChargenProgressionFeatureVM> m_HoveredFeature = new ReactiveProperty<ChargenProgressionFeatureVM>();

	private ReactiveProperty<int> m_CurrentLevel = new ReactiveProperty<int>();

	private ReactiveProperty<int> m_LastFinishedRank = new ReactiveProperty<int>();

	private ReactiveProperty<int> m_HoveredLevel = new ReactiveProperty<int>();

	private ReactiveProperty<bool> m_IsMinimized = new ReactiveProperty<bool>();

	public ChargenProgressionHeaderVM HeaderVM { get; private set; }

	public ReadOnlyReactiveProperty<bool> IsMinimized => m_IsMinimized;

	public IReadOnlyList<ChargenProgressionFeatureVM> FeatureRowVMs => m_FeatureRowVMs;

	public ChargenProgressionVM(ReadOnlyReactiveProperty<LevelUpManager> levelUpManager, ReadOnlyReactiveProperty<CharGenPhaseBaseVM> currentPhaseVM)
	{
		HeaderVM = new ChargenProgressionHeaderVM(m_CurrentLevel, m_LastFinishedRank, m_HoveredLevel, MAX_LEVELS, ToggleOpen);
		m_LevelUpManager = levelUpManager;
		currentPhaseVM.Subscribe(UpdateCurrent);
		SetLastFinishedRank(m_LevelUpManager.CurrentValue.RanksRange.From - 1);
		SetupFeatureRows();
		m_IsMinimized.Value = true;
	}

	public void SetCurrentHover(int x, int y)
	{
		m_HoveredLevel.Value = x + 1;
		m_HoveredFeature.Value = m_FeatureRowVMs[y - 1];
	}

	private void SetupFeatureRows()
	{
		m_LevelUpManager.CurrentValue.PreviewUnit.Progression.GetSelectionsByPath(m_LevelUpManager.CurrentValue.Path);
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.Career, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpFeature, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpSpecialization, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpAbility, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpUpgrade, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpModification, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpTalent, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.Characteristics, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
		m_FeatureRowVMs.Add(new ChargenProgressionFeatureVM(CharGenPhaseType.LevelUpSkill, m_LevelUpManager, m_HoveredFeature, m_LastFinishedRank));
	}

	private void UpdateCurrent(CharGenPhaseBaseVM phaseVM)
	{
		if (phaseVM == null)
		{
			m_CurrentLevel.Value = m_LevelUpManager.CurrentValue.RanksRange.From;
		}
		else
		{
			m_CurrentLevel.Value = Math.Abs(phaseVM.Rank);
		}
	}

	public void SetLastFinishedRank(int rank)
	{
		m_LastFinishedRank.Value = rank;
		m_LastFinishedRank.ForceNotify();
	}

	private void ToggleOpen()
	{
		m_IsMinimized.Value = !m_IsMinimized.Value;
		if (m_IsMinimized.Value)
		{
			m_HoveredLevel.Value = -1;
		}
	}
}
