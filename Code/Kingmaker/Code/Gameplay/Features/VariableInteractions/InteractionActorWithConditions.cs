using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.Interaction;
using Kingmaker.Localization;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

public struct InteractionActorWithConditions
{
	public readonly IInteractionVariantActor VariantActor;

	public readonly List<InteractionWithConditions.ShowReason> ShowReasons;

	public readonly ConditionsChecker SelectConditions;

	public readonly LocalizedString CannotSelectReason;

	public InteractionActorWithConditions(IInteractionVariantActor actor)
	{
		VariantActor = actor;
		ShowReasons = new List<InteractionWithConditions.ShowReason>();
		SelectConditions = null;
		CannotSelectReason = null;
	}

	public InteractionActorWithConditions(IInteractionVariantActor variantActor, List<InteractionWithConditions.ShowReason> showReasons, ConditionsChecker selectConditions, LocalizedString cannotSelectReason)
	{
		VariantActor = variantActor;
		ShowReasons = showReasons;
		SelectConditions = selectConditions;
		CannotSelectReason = cannotSelectReason;
	}

	public string GetReasonFor(ConditionsHolder passedCondition)
	{
		return ShowReasons.FirstOrDefault((InteractionWithConditions.ShowReason r) => r.Conditions.Get() == passedCondition)?.ShowHint?.Text;
	}
}
