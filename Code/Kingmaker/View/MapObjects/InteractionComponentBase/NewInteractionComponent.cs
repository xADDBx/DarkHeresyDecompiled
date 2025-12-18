using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[RequireComponent(typeof(MapObjectView))]
public abstract class NewInteractionComponent<TPart> : EntityPartComponent<TPart>, IInteractionComponent, IDialogReference where TPart : NewInteractionPart, new()
{
	public abstract float ProximityRadius { get; }

	public abstract DialogReferenceType GetUsagesFor(BlueprintDialog dialog);
}
[RequireComponent(typeof(MapObjectView))]
public abstract class NewInteractionComponent<TPart, TSettings> : EntityPartComponent<TPart, TSettings>, IInteractionComponent, IDialogReference where TPart : NewInteractionPart, new() where TSettings : class, new()
{
	public abstract float ProximityRadius { get; }

	public abstract DialogReferenceType GetUsagesFor(BlueprintDialog dialog);
}
