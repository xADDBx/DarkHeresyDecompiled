using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Interaction;

namespace Kingmaker.View.MapObjects;

public abstract class InteractionRestriction<TPart> : EntityPartComponent<TPart> where TPart : EntityPartWithConfig, new()
{
}
public abstract class InteractionRestriction<TPart, TSettings> : EntityPartComponent<TPart, TSettings> where TPart : EntityPartWithConfig, IInteractionVariantActor, new() where TSettings : class, new()
{
}
