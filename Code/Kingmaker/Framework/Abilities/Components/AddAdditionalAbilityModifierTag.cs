using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.Abilities.Components;

[Serializable]
[ClassInfoBox("Все модификаторы абилок содержащие тег SourceTag будут считаться содержащими еще и тег AdditionalTag")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Modifier/AddAdditionalAbilityModifierTag")]
[TypeId("44454e5207b046be9bc2aa6e1c4d8d6a")]
public sealed class AddAdditionalAbilityModifierTag : UnitFactComponentDelegate
{
	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> SourceTag;

	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> AdditionalTag;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityModifiers>().AddAdditionalModifierTag(SourceTag, AdditionalTag, base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveAdditionalModifierTag(base.Runtime);
	}
}
