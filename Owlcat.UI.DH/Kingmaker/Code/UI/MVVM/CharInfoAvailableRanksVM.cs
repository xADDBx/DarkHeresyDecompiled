using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAvailableRanksVM : ViewModel, ILevelUpManagerUIHandler, ISubscriber, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>
{
	private readonly ReactiveProperty<int> m_NextLevelExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentLevelExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_NewRanksCount = new ReactiveProperty<int>();

	private readonly CareerPathVM m_CareerPathVM;

	public ReadOnlyReactiveProperty<int> NextLevelExp => m_NextLevelExp;

	public ReadOnlyReactiveProperty<int> CurrentLevelExp => m_CurrentLevelExp;

	public ReadOnlyReactiveProperty<int> CurrentExp => m_CurrentExp;

	public ReadOnlyReactiveProperty<int> NewRanksCount => m_NewRanksCount;

	public bool IsInLevelupProcess => m_CareerPathVM.IsInLevelupProcess;

	public CharInfoAvailableRanksVM(CareerPathVM careerPathVM)
	{
		m_CareerPathVM = careerPathVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateData()
	{
		UpdateExp();
		UpdateLevel();
	}

	private void UpdateExp()
	{
		BaseUnitEntity unit = m_CareerPathVM.Unit;
		if (unit != null && !unit.IsDisposed)
		{
			BlueprintStatProgression experienceTable = ConfigRoot.Instance.Progression.ExperienceTable;
			m_NextLevelExp.Value = experienceTable.GetBonus(unit.Progression.CharacterLevel + 1);
			m_CurrentLevelExp.Value = experienceTable.GetBonus(unit.Progression.CharacterLevel);
			m_CurrentExp.Value = (unit.IsPet ? unit.Master.Progression.Experience : unit.Progression.Experience);
		}
	}

	private void UpdateLevel()
	{
		BaseUnitEntity unit = m_CareerPathVM.Unit;
		if (unit != null && !unit.IsDisposed)
		{
			int characterLevel = unit.Progression.CharacterLevel;
			int experienceLevel = unit.Progression.ExperienceLevel;
			int a = Math.Max(0, experienceLevel - characterLevel);
			a = Mathf.Min(a, m_CareerPathVM.MaxRank - m_CareerPathVM.CurrentRank.CurrentValue);
			m_NewRanksCount.Value = a;
		}
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		UpdateData();
	}

	public void HandleUICommitChanges()
	{
		UpdateData();
	}

	public void HandleUISelectionChanged()
	{
		UpdateData();
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateData();
	}
}
