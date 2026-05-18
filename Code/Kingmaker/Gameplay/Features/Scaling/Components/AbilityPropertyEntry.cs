using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Gameplay.Features.Scaling.Components;

[Serializable]
public class AbilityPropertyEntry
{
	private sealed class PropertyNameAttribute : EnumOrderByBlueprintAttribute
	{
		private static readonly Enum[] _AbilityOrder = new Enum[18]
		{
			ContextPropertyName.Value1,
			ContextPropertyName.Value2,
			ContextPropertyName.Dice1,
			ContextPropertyName.Dice2,
			ContextPropertyName.Bonus1,
			ContextPropertyName.Bonus2,
			ContextPropertyName.DamageDice1,
			ContextPropertyName.DamageDice2,
			ContextPropertyName.DamageBonus1,
			ContextPropertyName.DamageBonus2,
			ContextPropertyName.HealDice1,
			ContextPropertyName.HealDice2,
			ContextPropertyName.HealBonus1,
			ContextPropertyName.HealBonus2,
			ContextPropertyName.StatBonus1,
			ContextPropertyName.StatBonus2,
			ContextPropertyName.Duration1,
			ContextPropertyName.Duration2
		};

		private static readonly Enum[] _ModifierOrder = new Enum[18]
		{
			ContextPropertyName.ModValue1,
			ContextPropertyName.ModValue2,
			ContextPropertyName.ModDice1,
			ContextPropertyName.ModDice2,
			ContextPropertyName.ModBonus1,
			ContextPropertyName.ModBonus2,
			ContextPropertyName.ModDamageDice1,
			ContextPropertyName.ModDamageDice2,
			ContextPropertyName.ModDamageBonus1,
			ContextPropertyName.ModDamageBonus2,
			ContextPropertyName.ModHealDice1,
			ContextPropertyName.ModHealDice2,
			ContextPropertyName.ModHealBonus1,
			ContextPropertyName.ModHealBonus2,
			ContextPropertyName.ModStatBonus1,
			ContextPropertyName.ModStatBonus2,
			ContextPropertyName.ModDuration1,
			ContextPropertyName.ModDuration2
		};

		public override Enum[] Order => _AbilityOrder;

		public override Enum[] GetOrder(BlueprintScriptableObject blueprint)
		{
			if (!(blueprint is BlueprintAbilityModifier))
			{
				return _AbilityOrder;
			}
			return _ModifierOrder;
		}
	}

	[EnumPicker]
	[PropertyName]
	public ContextPropertyName Name;

	public SaveToContextType SaveToContext;

	public PropertyCalculator Value;

	public AbilityPropertyUISettings UISettings;
}
