using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoNameAndPortraitVM : CharInfoComponentWithLevelUpVM
{
	private readonly ReactiveProperty<string> m_UnitName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<CharInfoSummaryVM> m_SummaryVM = new ReactiveProperty<CharInfoSummaryVM>();

	private readonly ReactiveProperty<int> m_MeleeValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_RangedValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_DamageReductionValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<CareerPathVM> m_FirstCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<CareerPathVM> m_SecondCareer = new ReactiveProperty<CareerPathVM>();

	private readonly BuffGroupsVM m_BuffGroupsVM;

	private readonly List<CareerPathVM> m_CareerPaths = new List<CareerPathVM>(2);

	public readonly CharInfoLevelClassScoresVM LevelClassScoresVM;

	public readonly CharInfoSkillsAndWeaponsVM CharInfoSkillsAndWeaponsVM;

	public readonly CharInfoBuffGroupsVM CharInfoBuffGroupsVM;

	public ReadOnlyReactiveProperty<string> UnitName => m_UnitName;

	public ReadOnlyReactiveProperty<CharInfoSummaryVM> SummaryVM => m_SummaryVM;

	public ReadOnlyReactiveProperty<int> MeleeValue => m_MeleeValue;

	public ReadOnlyReactiveProperty<int> RangedValue => m_RangedValue;

	public ReadOnlyReactiveProperty<int> DamageReductionValue => m_DamageReductionValue;

	public ReadOnlyReactiveProperty<CareerPathVM> FirstCareer => m_FirstCareer;

	public ReadOnlyReactiveProperty<CareerPathVM> SecondCareer => m_SecondCareer;

	public CharInfoAbilityScoresBlockVM AbilityScores { get; }

	public Sprite UnitPortraitSmall => base.PreviewUnit.CurrentValue?.UISettings.Portrait?.SmallPortrait;

	public Sprite UnitPortraitHalf => base.PreviewUnit.CurrentValue?.UISettings.Portrait?.HalfLengthPortrait ?? UnitPortraitSmall;

	public Sprite UnitPortraitFull => base.PreviewUnit.CurrentValue?.UISettings.Portrait.FullLengthPortrait ?? UnitPortraitHalf;

	public CharInfoHitPointsVM HitPoints { get; }

	public CharInfoNameAndPortraitVM(UnitBuffBlockVM buffBlockVM, BuffGroupsVM buffGroupsVM, ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		HitPoints = new CharInfoHitPointsVM(unit).AddTo(this);
		LevelClassScoresVM = new CharInfoLevelClassScoresVM(Unit).AddTo(this);
		CharInfoSkillsAndWeaponsVM = new CharInfoSkillsAndWeaponsVM(Unit).AddTo(this);
		m_BuffGroupsVM = buffGroupsVM;
		CharInfoBuffGroupsVM = new CharInfoBuffGroupsVM(unit, buffBlockVM, buffGroupsVM).AddTo(this);
		m_SummaryVM.Value = new CharInfoSummaryVM(Unit, buffGroupsVM).AddTo(this);
		AbilityScores = new CharInfoAbilityScoresBlockVM(unit, levelUpManager).AddTo(this);
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		m_UnitName.Value = base.PreviewUnit.CurrentValue.CharacterName;
		int defenceValue = InspectExtensions.GetDefenceValue(base.PreviewUnit.CurrentValue);
		m_MeleeValue.Value = defenceValue;
		m_RangedValue.Value = defenceValue;
		m_DamageReductionValue.Value = base.PreviewUnit.CurrentValue.Actor.GetStat(StatType.ArmorDamageReduction, null, default(StatContext), "UpdateData");
		m_CareerPaths.Clear();
		(BlueprintCareerPath, int)[] array = base.PreviewUnit.CurrentValue.Progression.AllCareerPaths.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			CareerPathVM item = new CareerPathVM(Unit.CurrentValue, array[i].Item1, null);
			m_CareerPaths.Add(item);
		}
		m_FirstCareer.Value = ((m_CareerPaths.Count > 0) ? m_CareerPaths[0] : null);
		m_SecondCareer.Value = ((m_CareerPaths.Count > 1) ? m_CareerPaths[1] : null);
		UpdateTooltips();
	}

	public void SelectNextCharacter()
	{
		SelectCharacter(1);
	}

	public void SelectPrevCharacter()
	{
		SelectCharacter(-1);
	}

	private void SelectCharacter(int k)
	{
		List<BaseUnitEntity> actualGroup = Game.Instance.Controllers.SelectionCharacter.ActualGroup;
		int num = (actualGroup.IndexOf(Unit.CurrentValue) + k) % actualGroup.Count;
		if (num < 0)
		{
			num += actualGroup.Count;
		}
		Game.Instance.Controllers.SelectionCharacter.SetSelected(actualGroup[num]);
		if (actualGroup.Count == 1)
		{
			CombatSounds.Instance.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	private void UpdateTooltips()
	{
		_ = Unit.CurrentValue;
	}

	public override void HandleUISelectionChanged()
	{
		base.HandleUISelectionChanged();
		UpdateData();
	}
}
