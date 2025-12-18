using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Getters;

[TypeId("25e3d2c14e2b4756a6614415d5735f2c")]
public class IsCombatPreparationPhaseGetter : BoolPropertyGetter
{
	protected override bool GetBaseValue()
	{
		return Game.Instance.Controllers.TurnController.IsPreparationTurn;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is Combat Preparation Phase?";
	}
}
