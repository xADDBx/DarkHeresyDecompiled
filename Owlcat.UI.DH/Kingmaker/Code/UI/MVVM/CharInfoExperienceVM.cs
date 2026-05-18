using System;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoExperienceVM : CharInfoComponentVM, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IPartyCombatHandler, ILevelUpCompleteUIHandler
{
	private readonly ReactiveProperty<int> m_NextLevelExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentLevelExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentExp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<float> m_CurrentLevelExpRatio = new ReactiveProperty<float>();

	private readonly ReactiveProperty<bool> m_CanLevelup = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_Level = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_HasPsyRating = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_PsyRating = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_NewRanksCount = new ReactiveProperty<int>();

	public TooltipBaseTemplate PsyRatingTooltip;

	public ReadOnlyReactiveProperty<int> NextLevelExp => m_NextLevelExp;

	public ReadOnlyReactiveProperty<int> CurrentLevelExp => m_CurrentLevelExp;

	public ReadOnlyReactiveProperty<int> CurrentExp => m_CurrentExp;

	public ReadOnlyReactiveProperty<float> CurrentLevelExpRatio => m_CurrentLevelExpRatio;

	public ReadOnlyReactiveProperty<bool> CanLevelup => m_CanLevelup;

	public ReadOnlyReactiveProperty<int> Level => m_Level;

	public ReadOnlyReactiveProperty<bool> HasPsyRating => m_HasPsyRating;

	public ReadOnlyReactiveProperty<int> PsyRating => m_PsyRating;

	public ReadOnlyReactiveProperty<int> NewRanksCount => m_NewRanksCount;

	public CharInfoExperienceVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		if (Unit.CurrentValue != null)
		{
			UpdateData();
		}
	}

	private void UpdateData()
	{
		UpdateExp();
		UpdateLevel();
		UpdatePsyRating();
	}

	private void UpdateExp()
	{
		if (Unit.CurrentValue != null && !Unit.CurrentValue.IsDisposed)
		{
			BlueprintStatProgression experienceTable = ConfigRoot.Instance.Progression.ExperienceTable;
			m_NextLevelExp.Value = experienceTable.GetBonus(Unit.CurrentValue.Progression.CharacterLevel + 1);
			m_CurrentLevelExp.Value = experienceTable.GetBonus(Unit.CurrentValue.Progression.CharacterLevel);
			m_CurrentExp.Value = (Unit.CurrentValue.IsPet ? Unit.CurrentValue.Master.Progression.Experience : Unit.CurrentValue.Progression.Experience);
			int num = CurrentExp.CurrentValue - CurrentLevelExp.CurrentValue;
			int num2 = NextLevelExp.CurrentValue - CurrentLevelExp.CurrentValue;
			m_CurrentLevelExpRatio.Value = ((num2 > 0) ? ((float)num / (float)num2) : 0f);
		}
	}

	private void UpdateLevel()
	{
		int characterLevel = Unit.CurrentValue.Progression.CharacterLevel;
		m_Level.Value = characterLevel;
		m_CanLevelup.Value = Unit.CurrentValue.Progression.CanLevelUp && !UIUtilityCombat.IsCombatLockActive() && !RootUIContext.Instance.IsChargenShown;
		int experienceLevel = Unit.CurrentValue.Progression.ExperienceLevel;
		int value = Math.Max(0, experienceLevel - characterLevel);
		m_NewRanksCount.Value = value;
	}

	private void UpdatePsyRating()
	{
	}

	public void LevelUp()
	{
		CharGenConfig.Create(Unit.CurrentValue, CharGenMode.LevelUp).OpenUI();
	}

	public override void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		UpdateData();
	}

	public override void HandleDestroyLevelUpManager()
	{
		UpdateData();
	}

	public override void HandleUISelectCareerPath()
	{
		UpdateData();
	}

	public override void HandleUICommitChanges()
	{
		UpdateData();
	}

	public override void HandleUISelectionChanged()
	{
		UpdateData();
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateData();
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateData();
	}

	public void HandleLevelUpComplete()
	{
		UpdateData();
	}
}
