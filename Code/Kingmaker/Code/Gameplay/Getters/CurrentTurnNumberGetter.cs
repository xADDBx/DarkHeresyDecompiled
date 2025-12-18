using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Getters;

[Serializable]
[TypeId("f72bcd7b2df34eee9739f88160f7a4e5")]
public class CurrentTurnNumberGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return Game.Instance.Controllers.TurnController.CombatRound;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current Turn Number";
	}
}
