using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("6eff08e21be2770478bf1dd5523a5ad8")]
public class CheckAbilityTagGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BpRef<BlueprintAbilityTag> tag;

	protected override bool GetBaseValue()
	{
		return (EvalContext.Current.Ability?.OriginalBlueprint ?? null)?.Tags.Contains((BpRef<BlueprintAbilityTag> p) => p == tag) ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability has tag {tag}";
	}
}
