using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("79d294e04edc4b8b90a29fb25b46baab")]
public class TutorialTriggerCombatCompletedWithCritInjury : TutorialTrigger, ITurnBasedModeHandler, ISubscriber
{
	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased && Game.Instance.Player.Party.Any((BaseUnitEntity unit) => unit.BodyParts.Any((BlueprintBodyPart part) => unit.Health.GetCriticalStage(part) > 0)))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}
}
