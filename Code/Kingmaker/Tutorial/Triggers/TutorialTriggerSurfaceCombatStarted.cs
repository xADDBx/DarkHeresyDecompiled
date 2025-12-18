using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("edba194b5f694bfcb57475c59345dce5")]
public class TutorialTriggerSurfaceCombatStarted : TutorialTrigger, ITurnBasedModeHandler, ISubscriber
{
	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}
}
