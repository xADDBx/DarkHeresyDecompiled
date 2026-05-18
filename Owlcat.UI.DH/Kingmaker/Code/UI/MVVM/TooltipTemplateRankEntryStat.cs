using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TooltipTemplateRankEntryStat : TooltipTemplateStat
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly ReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	public TooltipTemplateRankEntryStat(StatTooltipData statData, FeatureSelectionItem featureSelectionItem, ReadOnlyReactiveProperty<SelectionStateFeature> selectionState, bool showCompanionStats = false)
		: base(statData, showCompanionStats)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = base.GetBody(type).ToList();
		CalculatedPrerequisite calculatedPrerequisite = m_SelectionState.CurrentValue?.GetCalculatedPrerequisite(m_SelectionItem);
		if (calculatedPrerequisite != null)
		{
			list.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			list.Add(new BrickPrerequisiteVM(UIUtilityAbilities.GetPrerequisiteEntries(calculatedPrerequisite)));
		}
		return list;
	}
}
