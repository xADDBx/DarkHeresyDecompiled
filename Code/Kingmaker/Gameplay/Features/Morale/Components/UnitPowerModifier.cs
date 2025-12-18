using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Morale.Components;

[Serializable]
[ComponentName("Morale/UnitPowerModifier")]
[TypeId("74c4efb29d2e4bf3b4abb2ef388e07bc")]
public sealed class UnitPowerModifier : UnitFactComponentDelegate
{
	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		ModifierType = ModifierType.PctMul_Extra,
		Enabled = true
	};

	protected override void OnActivateOrPostLoad()
	{
		Modifier.TryApply(base.Owner.GetOrCreate<PartMorale>().PowerFactorModifiers, base.Fact, ModifierDescriptor.UntypedStackable);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartMorale>()?.PowerFactorModifiers.RemoveAll((Modifier i) => i.Fact == base.Fact);
	}
}
