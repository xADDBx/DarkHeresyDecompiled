using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityInteracts
{
	public static IHasInteractionVariantActors GetIHasInteractionVariants([CanBeNull] MechanicEntity mechanicEntity)
	{
		if (mechanicEntity == null)
		{
			return null;
		}
		return mechanicEntity.Parts.GetAll<AbstractInteractionPart>().FirstOrDefault((AbstractInteractionPart i) => i is IHasInteractionVariantActors && i.Enabled) as IHasInteractionVariantActors;
	}

	public static int VariativeInteractionCount([CanBeNull] IMapObjectView mapObjectView)
	{
		if (mapObjectView == null)
		{
			return 0;
		}
		return (GetIHasInteractionVariants(mapObjectView.Data)?.GetInteractionVariantActors()?.Count()).GetValueOrDefault();
	}
}
