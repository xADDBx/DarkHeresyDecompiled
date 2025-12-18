using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Utility.Unit.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Code.Framework.Editor.Blueprints.BlueprintUnitTemplates;

[TypeId("d38dc21560a948b3ab2e761045329f5c")]
public class BlueprintUnitTemplate : BlueprintScriptableObject
{
	public UnitArchetype Archetype;

	public List<BlueprintUnitFactReference> MandatoryFacts = new List<BlueprintUnitFactReference>();

	public List<BlueprintAbilityReference> AvailableAbilities = new List<BlueprintAbilityReference>();

	public List<BlueprintUnitFactReference> AvailableFeatures = new List<BlueprintUnitFactReference>();

	public UnitStatModifiers StatModifiers = new UnitStatModifiers();

	public override void OnPostCreation()
	{
		UnitTemplateRoot.AddTemplate(this.ToReference<BlueprintUnitTemplateReference>());
	}
}
