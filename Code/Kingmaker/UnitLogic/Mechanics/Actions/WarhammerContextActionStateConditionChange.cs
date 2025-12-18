using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("58368f2782b790b48872e76b8c337ebe")]
public class WarhammerContextActionStateConditionChange : ContextAction
{
	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return "WarhammerContextActionStateConditionChange, invalid, do nothing";
	}
}
