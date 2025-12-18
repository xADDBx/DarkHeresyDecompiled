using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Code.Mechanics.Actions;

[Serializable]
[Obsolete]
[TypeId("fc279f2920ce411bbc3ad0c56f2cdcbf")]
public class ContextActionHealTraumas : ContextAction
{
	public int Stacks;

	public override string GetCaption()
	{
		return "Heal Traumas " + ((Stacks > 0) ? $"({Stacks} stacks)" : "(all stacks)");
	}

	protected override void RunAction()
	{
	}
}
