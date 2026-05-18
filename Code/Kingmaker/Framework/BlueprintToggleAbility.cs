using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework;

[Serializable]
[ComponentName("Ability/BlueprintToggleAbility")]
[TypeId("f0d3644c974e4479bc10691c799da451")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintToggleAbility : BlueprintUnitFact, IBlueprintScanner
{
	[ValidateNotNull]
	public BpRef<BlueprintMechanicEntityFact> Fact = new BpRef<BlueprintMechanicEntityFact>();

	public BpRef<BlueprintToggleAbilityGroup> Group = new BpRef<BlueprintToggleAbilityGroup>();

	public BpRef<BlueprintAbilityTag>[] AbilityModifierTags = new BpRef<BlueprintAbilityTag>[0];

	public RestrictionCalculator AbilityModifierRestriction = new RestrictionCalculator();

	[ValidateNoNullEntries]
	[InspectorReadOnly]
	public BpRef<BlueprintToggleAbility>[] Upgrades = new BpRef<BlueprintToggleAbility>[0];

	public override string Description => UtilityAbilities.GetLongOrShortText(base.Description, state: true);

	public bool HasBuffModifierTag
	{
		get
		{
			BpRef<BlueprintAbilityTag>[] abilityModifierTags = AbilityModifierTags;
			for (int i = 0; i < abilityModifierTags.Length; i++)
			{
				if (abilityModifierTags[i] == ConfigRoot.Instance.AbilityRoot.BuffAbilityTag)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override MechanicEntityFact CreateFact(IEvalContext? parentContext, BuffDuration duration, int rank = 1)
	{
		return new ToggleAbility(this);
	}

	public void Scan()
	{
	}
}
