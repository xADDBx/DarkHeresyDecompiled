using System;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;

namespace Kingmaker.Code.UI.MVVM;

public class NewInteractionVariantToggleVM : InteractionVariantVM
{
	public NewInteractionVariantToggleVM(InteractionActorWithConditions variantActor, string resourceName, string showReason, Action onInteract)
		: base(variantActor, resourceName, null, null, null, showReason, onInteract)
	{
	}

	public override string GetHint()
	{
		if (InteractionActor.SelectConditions != null && !InteractionActor.SelectConditions.Check())
		{
			return HandleNarratorText(InteractionActor.CannotSelectReason?.String.Text);
		}
		return HandleNarratorText(ShowReason);
	}
}
