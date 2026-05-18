using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScalingStatsVM : TooltipBrickVM
{
	private readonly MechanicEntity m_MechanicEntity;

	public readonly string DescriptionText;

	public readonly IReadOnlyList<AbilityStatData> Stats;

	public BrickAbilityScalingStatsVM(IEnumerable<StatType> stats, MechanicEntity entity)
	{
		m_MechanicEntity = entity;
		DescriptionText = UIStrings.Instance.Tooltips.AbilityScalingCharacteristics;
		Stats = stats.Select((StatType s) => new AbilityStatData(s, LocalizedTexts.Instance.Stats.GetShortText(s), entity.Actor.GetStat(s, null, default(StatContext), ".ctor").ModifiedValue)).ToList();
	}

	public TooltipBaseTemplate GetStatTooltip(AbilityStatData statData)
	{
		return new TooltipTemplateStat(StatTooltipData.FromActor(m_MechanicEntity, statData.StatType));
	}
}
