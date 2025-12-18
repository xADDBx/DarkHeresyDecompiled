using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Components.Etudes;

[Serializable]
[TypeId("f89799233b56463fb81c8afa5715999a")]
public sealed class EtudeMoraleVictoryTrigger : EtudeBracketTrigger, IMoraleVictoryConfirmationHandler, ISubscriber
{
	public ConditionsChecker Conditions = new ConditionsChecker();

	public ActionList Actions = new ActionList();

	void IMoraleVictoryConfirmationHandler.HandleMoraleVictoryConfirmation(bool confirmed)
	{
		if (base.AlreadyProcessedActivation && confirmed && Conditions.Check())
		{
			Actions.Run();
		}
	}
}
