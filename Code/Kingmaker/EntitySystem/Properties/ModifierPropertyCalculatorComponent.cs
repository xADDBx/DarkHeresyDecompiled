using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[TypeId("00785197ec424694b84682b2512b3caa")]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[AllowMultipleComponents]
public class ModifierPropertyCalculatorComponent : BlueprintComponent, IPropertyCalculatorComponent
{
	private sealed class PropertyNameAttribute : EnumOrderAttribute
	{
		private static readonly Enum[] _Order = new Enum[18]
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

		public override Enum[] Order => _Order;
	}

	[EnumPicker]
	[PropertyName]
	public ContextPropertyName Name = ContextPropertyName.ModValue1;

	public SaveToContextType SaveToContext;

	public PropertyCalculator Value;

	ContextPropertyName IPropertyCalculatorComponent.Name => Name;

	SaveToContextType IPropertyCalculatorComponent.SaveToContext => SaveToContext;

	public int GetValue(MechanicsContext context, MechanicEntity currentEntity)
	{
		return Value.GetValue(currentEntity, context);
	}

	public int GetValue(PropertyContext context)
	{
		return Value.GetValue(context);
	}
}
