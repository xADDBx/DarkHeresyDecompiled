using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[TypeId("b96d07cc91ca426c9b48bfef6f965788")]
public class ContextActionChangeVeilValue : ContextAction
{
	public ContextValue Value;

	public override string GetCaption()
	{
		return $"Changes Veil by {Value}";
	}

	protected override void RunAction()
	{
		Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.Custom, null, Value.Calculate(base.Context));
	}
}
