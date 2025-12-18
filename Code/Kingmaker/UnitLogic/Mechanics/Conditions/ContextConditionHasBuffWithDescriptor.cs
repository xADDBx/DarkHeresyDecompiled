using System;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("f822c5d972062e94f8f80a66c6834c7f")]
public class ContextConditionHasBuffWithDescriptor : ContextCondition
{
	public SpellDescriptorWrapper SpellDescriptor;

	protected override string GetConditionCaption()
	{
		return string.Concat("Check if target has buffs with Descriptor");
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
