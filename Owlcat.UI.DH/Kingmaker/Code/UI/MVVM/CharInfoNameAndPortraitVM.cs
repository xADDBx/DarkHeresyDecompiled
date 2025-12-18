using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoNameAndPortraitVM : CharInfoComponentWithLevelUpVM
{
	private readonly ReactiveProperty<string> m_UnitName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_PositiveStateTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<CharInfoSummaryVM> m_SummaryVM = new ReactiveProperty<CharInfoSummaryVM>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_NegativeStateTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_TraumasStateTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DamageOverTimeStateTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<int> m_MeleeValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_RangedValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_DamageReductionValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<CareerPathVM> m_FirstCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<CareerPathVM> m_SecondCareer = new ReactiveProperty<CareerPathVM>();

	private List<CareerPathVM> careerPaths = new List<CareerPathVM>(2);

	public readonly CharInfoLevelClassScoresVM LevelClassScoresVM;

	public readonly CharInfoSkillsAndWeaponsVM CharInfoSkillsAndWeaponsVM;

	public ReadOnlyReactiveProperty<string> UnitName => m_UnitName;

	public ReadOnlyReactiveProperty<CharInfoSummaryVM> SummaryVM => m_SummaryVM;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> PositiveStateTooltip => m_PositiveStateTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> NegativeStateTooltip => m_NegativeStateTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> TraumasStateTooltip => m_TraumasStateTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DamageOverTimeStateTooltip => m_DamageOverTimeStateTooltip;

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

	public CharInfoNameAndPortraitVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		HitPoints = new CharInfoHitPointsVM(unit).AddTo(this);
		LevelClassScoresVM = new CharInfoLevelClassScoresVM(Unit).AddTo(this);
		CharInfoSkillsAndWeaponsVM = new CharInfoSkillsAndWeaponsVM(Unit).AddTo(this);
		m_SummaryVM.Value = new CharInfoSummaryVM(Unit).AddTo(this);
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
		m_DamageReductionValue.Value = base.PreviewUnit.CurrentValue.GetStatOptional(StatType.ArmorDamageReduction);
		m_SummaryVM.Value = new CharInfoSummaryVM(Unit).AddTo(this);
		careerPaths.Clear();
		(BlueprintCareerPath, int)[] array = base.PreviewUnit.CurrentValue.Progression.AllCareerPaths.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			CareerPathVM item = new CareerPathVM(Unit.CurrentValue, array[i].Item1, null);
			careerPaths.Add(item);
		}
		m_FirstCareer.Value = ((careerPaths.Count > 0) ? careerPaths[0] : null);
		m_SecondCareer.Value = ((careerPaths.Count > 1) ? careerPaths[1] : null);
		SetStatusTooltips();
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
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	private void SetStatusTooltips()
	{
		if (Unit.CurrentValue == null)
		{
			return;
		}
		ObservableList<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(Unit.CurrentValue);
		if (buffsTooltipBricks != null)
		{
			Dictionary<BuffGroupType, ObservableList<ITooltipBrick>> dictionary = (from b in buffsTooltipBricks.OfType<TooltipBrickBuff>()
				group b by b.Group).ToDictionary((IGrouping<BuffGroupType, TooltipBrickBuff> g) => g.Key, (IGrouping<BuffGroupType, TooltipBrickBuff> g) => new ObservableList<ITooltipBrick>(g));
			if (dictionary.GetValueOrDefault(BuffGroupType.Positive) != null && dictionary.GetValueOrDefault(BuffGroupType.Positive).Count > 0)
			{
				m_PositiveStateTooltip.Value = new CharInfoStatusEffectsTemplate(Unit.CurrentValue, BuffGroupType.Positive);
			}
			if (dictionary.GetValueOrDefault(BuffGroupType.Negative) != null && dictionary.GetValueOrDefault(BuffGroupType.Negative).Count > 0)
			{
				m_NegativeStateTooltip.Value = new CharInfoStatusEffectsTemplate(Unit.CurrentValue, BuffGroupType.Negative);
			}
			if (dictionary.GetValueOrDefault(BuffGroupType.CriticalEffect) != null && dictionary.GetValueOrDefault(BuffGroupType.CriticalEffect).Count > 0)
			{
				m_TraumasStateTooltip.Value = new CharInfoStatusEffectsTemplate(Unit.CurrentValue, BuffGroupType.CriticalEffect);
			}
			if (dictionary.GetValueOrDefault(BuffGroupType.DOT) != null && dictionary.GetValueOrDefault(BuffGroupType.DOT).Count > 0)
			{
				m_DamageOverTimeStateTooltip.Value = new CharInfoStatusEffectsTemplate(Unit.CurrentValue, BuffGroupType.DOT);
			}
		}
	}

	public override void HandleUISelectionChanged()
	{
		base.HandleUISelectionChanged();
		UpdateData();
	}
}
