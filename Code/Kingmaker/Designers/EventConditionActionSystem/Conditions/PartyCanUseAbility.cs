using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("c8fa2b21f19151143929070e56027a25")]
public class PartyCanUseAbility : Condition
{
	public AbilityDescription Description;

	public bool AllowItems;

	protected override string GetConditionCaption()
	{
		return string.Format("Can party use ability{0}: {1}", AllowItems ? " (items allowed)" : "", Description);
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
