using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FeatureSelectorSlotVM : CharInfoFeatureVM
{
	public FeatureSelectorSlotVM(Ability ability, MechanicEntity unit)
		: base(ability, unit)
	{
		m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_Tooltip.Value = new TooltipTemplateAbility(ability.Data, isScreenWindowTooltip: true);
	}
}
