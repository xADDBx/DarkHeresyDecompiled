using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpFittingAbilities : ITooltipBrick
{
	private BlueprintAbilityModifier m_BlueprintModifier;

	private BaseUnitEntity m_Caster;

	public TooltipBrickLevelUpFittingAbilities(BlueprintAbilityModifier blueprintModifier, BaseUnitEntity caster)
	{
		m_BlueprintModifier = blueprintModifier;
		m_Caster = caster;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpFittingAbilitiesVM(m_BlueprintModifier, m_Caster);
	}
}
