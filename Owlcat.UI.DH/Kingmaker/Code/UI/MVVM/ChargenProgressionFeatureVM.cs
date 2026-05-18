using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionFeatureVM : ViewModel
{
	private readonly CharGenPhaseType m_PhaseType;

	private Dictionary<int, ChargenProgressionFeatureLevelVM> m_LevelVMs = new Dictionary<int, ChargenProgressionFeatureLevelVM>();

	private ReactiveProperty<bool> m_isHovered = new ReactiveProperty<bool>();

	private ReadOnlyReactiveProperty<LevelUpManager> m_LevelUpManager;

	private ReactiveProperty<int> m_LevelUpdated = new ReactiveProperty<int>();

	private IEnumerable<FeatureSelectionData> m_PrevSelections;

	private IEnumerable<Feature> m_PrevSkills;

	private IEnumerable<Feature> m_PrevStats;

	private IEnumerable<StatType> m_CachedStatTypes;

	public string Title { get; private set; }

	public IReadOnlyDictionary<int, ChargenProgressionFeatureLevelVM> LevelVMs => m_LevelVMs;

	public ReadOnlyReactiveProperty<bool> IsHovered => m_isHovered;

	public ReadOnlyReactiveProperty<int> LevelUpdated => m_LevelUpdated;

	public ChargenProgressionFeatureVM(CharGenPhaseType phaseType, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager, ReadOnlyReactiveProperty<ChargenProgressionFeatureVM> m_HoveredFeature, ReactiveProperty<int> lastFinishedRank)
	{
		Title = UIStrings.Instance.CharGen.GetPhaseName(phaseType);
		m_PhaseType = phaseType;
		m_LevelUpManager = levelUpManager;
		m_CachedStatTypes = Enum.GetValues(typeof(StatType)).Cast<StatType>();
		m_PrevSelections = m_LevelUpManager.CurrentValue.PreviewUnit.Progression.GetSelectionsByPath(m_LevelUpManager.CurrentValue.Path);
		m_LevelUpManager.CurrentValue.PreviewUnit.Progression.Features.Enumerable.Select((Feature f) => f.Blueprint);
		m_PrevSkills = m_LevelUpManager.CurrentValue.PreviewUnit.Progression.Features.Enumerable.Where((Feature f) => f.Blueprint is BlueprintSkillAdvancement).ToList();
		m_PrevStats = m_LevelUpManager.CurrentValue.PreviewUnit.Progression.Features.Enumerable.Where((Feature f) => f.Blueprint is BlueprintStatAdvancement).Except(m_PrevSkills).ToList();
		UpdateLevels(1, 25);
		m_HoveredFeature.Subscribe(delegate(ChargenProgressionFeatureVM f)
		{
			m_isHovered.Value = f == this;
		}).AddTo(this);
		lastFinishedRank.DelayFrame(2).Subscribe(delegate
		{
			UpdateLevels(m_LevelUpManager.CurrentValue.RanksRange.From, m_LevelUpManager.CurrentValue.RanksRange.To);
		}).AddTo(this);
	}

	private void CreateLevelVM(BlueprintPath.RankEntry rankEntry, CharGenPhaseType phaseType, int index)
	{
		m_LevelVMs[index] = null;
		switch (phaseType)
		{
		case CharGenPhaseType.LevelUpFeature:
			if (rankEntry.Features.FirstOrDefault() != null)
			{
				m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index);
			}
			break;
		case CharGenPhaseType.Characteristics:
			if (rankEntry.Selections.FirstOrDefault((BlueprintSelection s) => s is BlueprintSelectionAttributes) != null)
			{
				BlueprintStatAdvancement blueprintStatAdvancement = (from s in m_PrevStats
					where s.FirstSource.PathRank == index
					select s.Blueprint as BlueprintStatAdvancement).FirstOrDefault();
				StatType currentStat2 = GetCurrentStat<BlueprintSelectionAttributes>(index);
				StatType statType2 = blueprintStatAdvancement?.Stat ?? currentStat2;
				string acronym = null;
				TooltipBaseTemplate tooltip2 = null;
				if (statType2 != 0)
				{
					acronym = LocalizedTexts.Instance.Stats.GetShortText(statType2);
					tooltip2 = new TooltipTemplateStat(StatTooltipData.FromActor(m_LevelUpManager.CurrentValue.PreviewUnit, statType2));
				}
				m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index, null, null, acronym, null, tooltip2);
			}
			break;
		case CharGenPhaseType.LevelUpSkill:
			if (rankEntry.Selections.FirstOrDefault((BlueprintSelection s) => s is BlueprintSelectionSkills) != null)
			{
				BlueprintSkillAdvancement blueprintSkillAdvancement = (from s in m_PrevSkills
					where s.FirstSource.PathRank == index
					select s.Blueprint as BlueprintSkillAdvancement).FirstOrDefault();
				StatType currentStat = GetCurrentStat<BlueprintSelectionSkills>(index);
				StatType statType = blueprintSkillAdvancement?.Stat ?? currentStat;
				string label = null;
				TooltipBaseTemplate tooltip = null;
				if (statType != 0)
				{
					label = LocalizedTexts.Instance.Stats.GetText(statType);
					tooltip = new TooltipTemplateStat(StatTooltipData.FromActor(m_LevelUpManager.CurrentValue.PreviewUnit, statType));
				}
				m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index, null, label, null, null, tooltip);
			}
			break;
		default:
		{
			if (rankEntry.Selections.FirstOrDefault((BlueprintSelection s) => s is BlueprintSelectionFeature blueprintSelectionFeature && blueprintSelectionFeature.Group == PhaseTypeToFeatureGroup(phaseType)) == null)
			{
				break;
			}
			m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index);
			TooltipBaseTemplate tooltipBaseTemplate = null;
			FeatureGroup featureGroup = PhaseTypeToFeatureGroup(phaseType);
			FeatureSelectionData featureSelectionData = m_PrevSelections.FirstOrDefault((FeatureSelectionData s) => s.Level == index && s.Selection.Group == featureGroup);
			SelectionStateFeature selectionStateFeature = m_LevelUpManager.CurrentValue.Selections.FirstOrDefault((SelectionState s) => s.PathRank == index && s is SelectionStateFeature selectionStateFeature2 && selectionStateFeature2.Blueprint.Group == featureGroup && selectionStateFeature2.SelectionItem.HasValue) as SelectionStateFeature;
			BlueprintFeature blueprintFeature = featureSelectionData.Feature ?? selectionStateFeature?.SelectionItem.Value.Feature;
			if (blueprintFeature == null)
			{
				break;
			}
			if (blueprintFeature.TryGetComponent<AddAvailableAbilityModifier>(out var component))
			{
				BlueprintAbilityModifier blueprint = component.Modifier.Blueprint;
				BlueprintAbilityTag blueprintAbilityTag = blueprint.Tags.FirstOrDefault();
				tooltipBaseTemplate = new TooltipTemplateLevelUpModifier(blueprint, null, m_LevelUpManager.CurrentValue.PreviewUnit);
				if (blueprintAbilityTag != null)
				{
					m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index, blueprintAbilityTag.Icon, blueprintAbilityTag.Name.Text, null, null, tooltipBaseTemplate);
					break;
				}
			}
			tooltipBaseTemplate = new TooltipTemplateUIFeature(new UIFeature(blueprintFeature));
			if ((phaseType == CharGenPhaseType.LevelUpAbility || phaseType == CharGenPhaseType.LevelUpUpgrade) && blueprintFeature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddFacts) is AddFacts { Facts: var facts } addFacts && facts[0] != null && addFacts.Facts[0] is BlueprintAbility blueprintAbility)
			{
				tooltipBaseTemplate = new TooltipTemplateAbility(blueprintAbility);
			}
			if (phaseType == CharGenPhaseType.LevelUpSpecialization)
			{
				tooltipBaseTemplate = new TooltipTemplateLevelUpSpecialization(new UIFeature(blueprintFeature), null, m_LevelUpManager.CurrentValue);
			}
			m_LevelVMs[index] = new ChargenProgressionFeatureLevelVM(index, blueprintFeature.Icon, blueprintFeature.name, UIUtilityAbilities.GetAbilityAcronym(blueprintFeature.Name), blueprintFeature.TalentIconInfo, tooltipBaseTemplate);
			break;
		}
		}
		m_LevelUpdated.Value = index;
		m_LevelUpdated.ForceNotify();
	}

	private void UpdateLevels(int from, int to)
	{
		for (int i = from; i <= to; i++)
		{
			BlueprintPath.RankEntry rankEntry = m_LevelUpManager.CurrentValue.Path?.GetRankEntry(i);
			if (rankEntry != null)
			{
				CreateLevelVM(rankEntry, m_PhaseType, i);
			}
		}
	}

	private FeatureGroup PhaseTypeToFeatureGroup(CharGenPhaseType phaseType)
	{
		return phaseType switch
		{
			CharGenPhaseType.LevelUpAbility => FeatureGroup.ActiveAbility, 
			CharGenPhaseType.LevelUpUpgrade => FeatureGroup.AbilityUpgrade, 
			CharGenPhaseType.LevelUpModification => FeatureGroup.Modifier, 
			CharGenPhaseType.LevelUpSpecialization => FeatureGroup.Specialization, 
			CharGenPhaseType.LevelUpTalent => FeatureGroup.Talent, 
			_ => FeatureGroup.None, 
		};
	}

	private StatType GetCurrentStat<TStatType>(int rank) where TStatType : BlueprintSelectionStats
	{
		if (m_LevelUpManager.CurrentValue.Selections.FirstOrDefault((SelectionState s) => s.PathRank == rank && s is SelectionStateStats selectionStateStats2 && selectionStateStats2.Blueprint is TStatType) is SelectionStateStats selectionStateStats)
		{
			foreach (StatType cachedStatType in m_CachedStatTypes)
			{
				if (selectionStateStats.GetPointsSpent(cachedStatType) > 0)
				{
					return cachedStatType;
				}
			}
		}
		return StatType.Unknown;
	}
}
