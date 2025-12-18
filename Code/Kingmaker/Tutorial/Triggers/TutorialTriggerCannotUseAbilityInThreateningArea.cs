using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("90fb04ed00a24c1ebf59ba65bd4b054f")]
public class TutorialTriggerCannotUseAbilityInThreateningArea : TutorialTrigger, IAbilityCannotUseInThreateningArea, ISubscriber
{
	public void HandleCannotUseAbilityInThreateningArea(AbilityData ability)
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			context.SourceAbility = ability;
			context.SourceUnit = ability.Caster as BaseUnitEntity;
		});
	}
}
