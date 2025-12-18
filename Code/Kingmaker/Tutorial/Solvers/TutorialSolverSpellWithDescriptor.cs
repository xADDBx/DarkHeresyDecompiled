using System;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Solvers;

[Obsolete]
[TypeId("c6ec8f6cde64e014691213dca4c32c1a")]
public class TutorialSolverSpellWithDescriptor : TutorialSolverSpellOrUsableItem
{
	public SpellDescriptorWrapper SpellDescriptors;

	[InfoBox(Text = "If NeedAllDescriptors is true, only buff that has all listed flags will trigger")]
	public bool NeedAllDescriptors;

	[InfoBox(Text = "To prevent including self healing spells or buffs")]
	public bool ExcludeOnlySelfTarget;

	protected override int GetPriority(AbilityData ability)
	{
		return GetSpellPriority(ability.Blueprint);
	}

	protected override int GetPriority(ItemEntity item)
	{
		return -1;
	}

	private int GetSpellPriority(BlueprintAbilityWrapper ability)
	{
		return -1;
	}
}
