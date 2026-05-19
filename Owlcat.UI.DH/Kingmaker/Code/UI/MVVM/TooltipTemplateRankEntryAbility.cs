using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateRankEntryAbility : TooltipTemplateAbility
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly ReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	private readonly RankEntrySelectionVM m_Owner;

	private CalculatedPrerequisite Prerequisite => m_SelectionState.CurrentValue?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, (BaseUnitEntity)base.Caster);

	public TooltipTemplateRankEntryAbility(BlueprintAbility blueprintAbility, FeatureSelectionItem featureSelectionItem, ReadOnlyReactiveProperty<SelectionStateFeature> selectionState, RankEntrySelectionVM owner, MechanicEntity caster)
		: base(blueprintAbility, null, caster)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Owner = owner;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = base.GetBody(type).ToList();
		if (Prerequisite != null)
		{
			list.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			list.Add(new BrickPrerequisiteVM(UIUtilityAbilities.GetPrerequisiteEntries(Prerequisite)));
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type != 0 && Game.Instance.IsControllerMouse && RankEntrySelectionFeaturesUtils.HasPrerequisiteFooter(Prerequisite, m_Owner))
		{
			yield return new BrickTitleVM(UIStrings.Instance.Tooltips.PrerequisitesFooter, TooltipTitleType.H6);
		}
	}
}
