using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Framework.Interaction;

public static class InteractionProcessPartExtensions
{
	public static bool HasActiveInteraction(this AbstractUnitEntity? entity)
	{
		return (entity?.GetOptional<InteractionProcessPart>())?.HasActiveInteraction ?? false;
	}
}
