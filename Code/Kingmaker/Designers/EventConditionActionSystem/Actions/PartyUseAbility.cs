using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("bd6efada588c86e4fbe9525e0674a2a7")]
public class PartyUseAbility : GameAction
{
	public AbilityDescription Description;

	public bool AllowItems;

	public override string GetCaption()
	{
		return string.Format("Party use ability{0}: {1}", AllowItems ? " (items allowed)" : "", Description);
	}

	protected override void RunAction()
	{
	}
}
