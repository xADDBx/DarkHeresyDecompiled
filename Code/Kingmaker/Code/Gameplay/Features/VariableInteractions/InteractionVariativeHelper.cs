using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

public static class InteractionVariativeHelper
{
	[CanBeNull]
	public static Entity GetEntity(this AbstractEntityPartComponent component)
	{
		return (Entity)EntityService.Instance.GetEntity(component.EntityId);
	}

	[CanBeNull]
	public static TPart GetEntityPart<TPart>(this AbstractEntityPartComponent component) where TPart : EntityPart
	{
		Entity entity = component.GetEntity();
		if (entity == null)
		{
			return null;
		}
		return entity.GetOptional<TPart>(component.EntityPartType);
	}
}
