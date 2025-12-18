using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[ComponentName("Stats/StatAdvancement")]
[AllowedOn(typeof(BlueprintStatAdvancement))]
[TypeId("1193f055d9ae4f5aa45e27400d7411cd")]
public class StatAdvancement : UnitFactComponentDelegate
{
	private BlueprintStatAdvancement Settings => (BlueprintStatAdvancement)base.Fact.Blueprint;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetStat(Settings.Stat).AddModifier(Settings.ValuePerRank * base.Fact.GetRank(), base.Runtime, Settings.ModifierDescriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Settings.Stat).RemoveModifiersFrom(base.Runtime);
	}
}
