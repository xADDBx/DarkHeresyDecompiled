using System;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[Serializable]
public class PhenomenaModificationEntry
{
	public PhenomenaModificationType Type;

	[ShowIf("IsAbilityVisible")]
	public BpRef<BlueprintAbility> Ability;

	[ShowIf("IsAdd")]
	public BlueprintPsykerRoot.PhenomenaData AddData;

	[ShowIf("IsReweight")]
	public float NewWeight = 1f;

	private bool IsAdd => Type == PhenomenaModificationType.Add;

	private bool IsReweight => Type == PhenomenaModificationType.Reweight;

	private bool IsAbilityVisible
	{
		get
		{
			if (Type != PhenomenaModificationType.Ban)
			{
				return Type == PhenomenaModificationType.Reweight;
			}
			return true;
		}
	}
}
