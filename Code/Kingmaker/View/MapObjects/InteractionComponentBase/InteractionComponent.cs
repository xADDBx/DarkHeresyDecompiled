using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.View.MapObjects.Traps;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[RequireComponent(typeof(MapObjectView))]
public abstract class InteractionComponent<TPart, TSettings> : EntityPartComponent<TPart, TSettings>, IInteractionComponent, IDialogReference where TPart : InteractionPart<TSettings>, new() where TSettings : InteractionSettings, new()
{
	float IInteractionComponent.ProximityRadius => Settings.ProximityRadius;

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Settings.Dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}

	protected override void Awake()
	{
		base.Awake();
		TrapObjectView trapObjectView = Settings?.Trap;
		if ((object)trapObjectView != null)
		{
			trapObjectView.TrappedObject = base.EntityView as MapObjectView;
		}
	}
}
