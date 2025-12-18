using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntrySelectionStatVM : RankEntrySelectionFeatureVM
{
	public readonly string StatDisplayName;

	public readonly string ShortName;

	private readonly ReactiveProperty<string> m_StatIncreaseLabel = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SummaryStatIncreaseLabel = new ReactiveProperty<string>();

	private readonly BlueprintStatAdvancement m_StatAdvancement;

	private readonly ModifiableValue m_UnitStat;

	public ReadOnlyReactiveProperty<string> StatIncreaseLabel => m_StatIncreaseLabel;

	public ReadOnlyReactiveProperty<string> SummaryStatIncreaseLabel => m_SummaryStatIncreaseLabel;

	public RankEntrySelectionStatVM(RankEntrySelectionVM owner, CareerPathVM careerPathVM, FeatureSelectionItem featureSelectionItem, ReadOnlyReactiveProperty<SelectionStateFeature> selectionState, Action<FeatureSelectionItem?> selectFeature)
		: base(owner, careerPathVM, featureSelectionItem, selectionState, selectFeature)
	{
		if (base.Feature is BlueprintStatAdvancement blueprintStatAdvancement)
		{
			StatDisplayName = LocalizedTexts.Instance.Stats.GetText(blueprintStatAdvancement.Stat);
			ShortName = UIUtilityText.GetStatShortName(blueprintStatAdvancement.Stat);
			m_StatAdvancement = blueprintStatAdvancement;
			m_UnitStat = owner.UnitProgressionVM.Unit.CurrentValue.Stats.GetStat(blueprintStatAdvancement.Stat);
			AddDisposable(ObservableSubscribeExtensions.Subscribe(OnUpdateState, delegate
			{
				UpdateIncreaseLabel();
			}));
			SetTooltip();
			UpdateIncreaseLabel();
		}
	}

	private void UpdateIncreaseLabel()
	{
		m_StatIncreaseLabel.Value = FormatStat(m_StatAdvancement);
		LevelUpManager levelUpManager = Owner.UnitProgressionVM.LevelUpManager;
		BaseUnitEntity obj = levelUpManager?.TargetUnit ?? Owner.UnitProgressionVM.Unit.CurrentValue;
		BaseUnitEntity baseUnitEntity = levelUpManager?.PreviewUnit;
		ModifiableValue stat = obj.Stats.GetStat(m_StatAdvancement.Stat);
		ModifiableValue obj2 = baseUnitEntity?.Stats?.GetStat(m_StatAdvancement.Stat);
		m_SummaryStatIncreaseLabel.Value = $"{stat.ModifiedValue} > {obj2?.ModifiedValue}";
	}

	private void SetTooltip()
	{
		ModifiableValue unitStat = m_UnitStat;
		StatTooltipData statTooltipData = default(StatTooltipData);
		if (!(unitStat is ModifiableValueAttributeStat attribute))
		{
			if (!(unitStat is ModifiableValueSkill skill))
			{
				if (unitStat != null)
				{
					statTooltipData = new StatTooltipData(m_UnitStat);
				}
				else
				{
					global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(unitStat);
				}
			}
			else
			{
				statTooltipData = new StatTooltipData(skill);
			}
		}
		else
		{
			statTooltipData = new StatTooltipData(attribute);
		}
		StatTooltipData statData = statTooltipData;
		m_Tooltip.Value = new TooltipTemplateRankEntryStat(statData, SelectionItem, SelectionState, showCompanionStats: true);
	}

	private string FormatStat(BlueprintStatAdvancement statAdvancement)
	{
		return "+" + statAdvancement.ValuePerRank;
	}
}
