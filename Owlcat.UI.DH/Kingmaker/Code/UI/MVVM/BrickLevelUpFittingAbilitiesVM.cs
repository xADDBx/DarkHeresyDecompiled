using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpFittingAbilitiesVM : TooltipBrickVM
{
	private readonly BlueprintAbilityModifier m_BlueprintModifier;

	private readonly BaseUnitEntity m_Unit;

	private readonly PartAbilityModifiers m_PartAbilityModifiers;

	private readonly List<TooltipElementAbilityWithModifierVM> m_Abilities = new List<TooltipElementAbilityWithModifierVM>();

	public IReadOnlyList<TooltipElementAbilityWithModifierVM> Abilities => m_Abilities;

	public BrickLevelUpFittingAbilitiesVM(BlueprintAbilityModifier blueprintModifier, BaseUnitEntity unit)
	{
		m_BlueprintModifier = blueprintModifier;
		m_Unit = unit;
		m_PartAbilityModifiers = m_Unit.GetOptional<PartAbilityModifiers>();
		Init();
	}

	private void Init()
	{
		m_Abilities.AddRange((from ability in m_Unit.Abilities.Visible
			where !ability.Blueprint.IsWeaponAbility && m_PartAbilityModifiers.IsSuitableModifier(m_BlueprintModifier, ability) && !ability.Data.Blueprint.IsBroken && !ability.Data.Blueprint.IsHeroic
			select new TooltipElementAbilityWithModifierVM(ability, m_PartAbilityModifiers)).ToList());
		m_Abilities.AddRange((from t in m_Unit.ToggleAbilities.Visible
			where m_PartAbilityModifiers.IsSuitableModifier(m_BlueprintModifier, t)
			select new TooltipElementAbilityWithModifierVM(t, m_PartAbilityModifiers)).ToList());
		if (m_BlueprintModifier.Match(ConfigRoot.Instance.AbilityRoot.AttackAbilityTag))
		{
			m_Abilities.Add(new TooltipElementAbilityWithModifierVM(ConfigRoot.Instance.AbilityRoot.WeaponSingleShotTag, m_PartAbilityModifiers));
			m_Abilities.Add(new TooltipElementAbilityWithModifierVM(ConfigRoot.Instance.AbilityRoot.WeaponBurstTag, m_PartAbilityModifiers));
			m_Abilities.Add(new TooltipElementAbilityWithModifierVM(ConfigRoot.Instance.AbilityRoot.WeaponAoETag, m_PartAbilityModifiers));
		}
	}
}
