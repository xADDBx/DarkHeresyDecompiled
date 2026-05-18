using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[Obsolete]
[TypeId("f9bf4ae9ccd847689d5cdf2e86bc51ca")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintPlayerUpgrader))]
[AllowedOn(typeof(ContextActionsList))]
[AllowMultipleComponents]
public class PropertyCalculatorComponent : BlueprintComponent, IPropertyCalculatorComponent
{
	private sealed class PropertyNameAttribute : EnumOrderAttribute
	{
		private static readonly Enum[] _Order = new Enum[18]
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

		public override Enum[] Order => _Order;
	}

	[EnumPicker]
	[PropertyName]
	public ContextPropertyName Name;

	public SaveToContextType SaveToContext;

	[VerticalLayout]
	public PropertyCalculator Value;

	ContextPropertyName IPropertyCalculatorComponent.Name => Name;

	SaveToContextType IPropertyCalculatorComponent.SaveToContext => SaveToContext;

	public int GetValue(IEvalContext context, MechanicEntity currentEntity)
	{
		return Value.GetValue(currentEntity, context);
	}
}
