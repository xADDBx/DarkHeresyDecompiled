using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e20d2ab26cb2403fb4971555bc91bc4c")]
public class ContextActionRequestEndTurn : ContextAction
{
	public override string GetCaption()
	{
		return "Request end turn for caster";
	}

	protected override void RunAction()
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit == base.Context.Caster)
		{
			Game.Instance.Controllers.TurnController.RequestEndTurn();
		}
	}
}
