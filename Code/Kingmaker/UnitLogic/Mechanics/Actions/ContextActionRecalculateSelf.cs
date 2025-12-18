using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("20eef6901e3c38a48b2e988dc13635a7")]
public class ContextActionRecalculateSelf : ContextAction
{
	protected override void RunAction()
	{
		base.Context.Fact?.Reapply();
	}

	public override string GetCaption()
	{
		return "Recalculate this fact";
	}
}
