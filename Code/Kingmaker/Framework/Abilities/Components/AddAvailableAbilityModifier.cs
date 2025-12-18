using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Abilities.Components;

[Serializable]
[ComponentName("Modifier/AddAvailableAbilityModifier")]
[TypeId("f6ff3ab5e50e46718698a2b65d560a93")]
public sealed class AddAvailableAbilityModifier : MechanicEntityFactComponentDelegate
{
	public BpRef<BlueprintAbilityModifier> Modifier;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityModifiers>().AddAvailableModifier(Modifier, base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveAvailableModifier(base.Runtime);
	}
}
